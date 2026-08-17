import { test, expect, type APIRequestContext, type BrowserContext } from '@playwright/test';

const LIMA_A = { latitude: -12.0464, longitude: -77.0428 };
const LIMA_B = { latitude: -12.0213, longitude: -77.0148 };
const NAME = 'Marcos Rodríguez';
const LICENCIA = 'CAT-04-88219';

async function createConductor(request: APIRequestContext): Promise<number> {
  const existing = await request.get('/api/conductores');
  if (existing.ok()) {
    const conductors = (await existing.json()) as { id: number; licencia: string }[];
    const match = conductors.find((c) => c.licencia === LICENCIA);
    if (match) {
      return match.id;
    }
  }

  const res = await request.post('/api/conductores', {
    data: { nombre: NAME, licencia: LICENCIA, telefono: '809-555-0192' },
  });
  expect(res.ok()).toBeTruthy();
  const body = (await res.json()) as { id: number };
  return body.id;
}

async function pollLive(
  request: APIRequestContext,
  conductorId: number,
  predicate: (lat: number, lng: number) => boolean,
  timeoutMs = 60_000,
): Promise<void> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const res = await request.get('/api/tracking/live');
    if (res.ok()) {
      const live = (await res.json()) as {
        conductorId: number;
        latitude: number;
        longitude: number;
      }[];
      const item = live.find((l) => l.conductorId === conductorId);
      if (item && predicate(item.latitude, item.longitude)) {
        return;
      }
    }
    await new Promise((r) => setTimeout(r, 1500));
  }
  throw new Error('La API no recibió la ubicación esperada.');
}

test.describe('Flujo GPS: conductor transmite, admin ve el marker moverse', () => {
  let conductorId: number;

  test.beforeAll(async ({ request }) => {
    conductorId = await createConductor(request);

    const active = await request.get(`/api/tracking/conductores/${conductorId}/active-session`);
    if (active.ok()) {
      const session = (await active.json()) as { sessionId: number; status: string } | null;
      if (session && session.status === 'active') {
        await request.post(`/api/tracking/sessions/${session.sessionId}/stop?conductorId=${conductorId}`);
      }
    }
  });

  test('driver envía pings y el admin ve el marker en A y luego en B', async ({ request, browser }) => {
    const driverContext: BrowserContext = await browser.newContext({
      geolocation: LIMA_A,
      permissions: ['geolocation'],
    });

    const driverPage = await driverContext.newPage();
    await driverPage.goto('/tracking');

    await driverPage.locator('.tracking-field select').selectOption({ label: `${NAME} · ${LICENCIA}` });
    await expect(driverPage.locator('.tracking-card h3')).toContainText(NAME);

    await driverPage.locator('.btn-primary', { hasText: 'Iniciar seguimiento' }).click();
    await expect(driverPage.locator('.gps-banner.active')).toContainText('GPS activo');

    await pollLive(request, conductorId, (lat, lng) =>
      Math.abs(lat - LIMA_A.latitude) < 0.01 && Math.abs(lng - LIMA_A.longitude) < 0.01,
    );

    await driverPage.waitForTimeout(12_000);
    await driverContext.setGeolocation(LIMA_B);
    await pollLive(request, conductorId, (lat, lng) =>
      Math.abs(lat - LIMA_B.latitude) < 0.01 && Math.abs(lng - LIMA_B.longitude) < 0.01,
    );

    const adminContext: BrowserContext = await browser.newContext();
    const adminPage = await adminContext.newPage();
    await adminPage.goto('/');
    await adminPage.locator('.tabs button', { hasText: 'GPS' }).click();

    await expect(adminPage.locator('.driver-item', { hasText: NAME })).toBeVisible();

    const map = adminPage.getByTestId('gps-map');
    await expect(map).toBeVisible();

    const lat = Number(await map.getAttribute('data-lat'));
    const lng = Number(await map.getAttribute('data-lng'));

    expect(Math.abs(lat - LIMA_B.latitude)).toBeLessThan(0.01);
    expect(Math.abs(lng - LIMA_B.longitude)).toBeLessThan(0.01);

    await driverPage.close();
    await driverContext.close();
    await adminContext.close();
  });
});