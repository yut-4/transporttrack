import { Api, ApiError } from './_lib/app';
import { getDataStore } from './_lib/store';

const CORS_HEADERS: Record<string, string> = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
  'Access-Control-Allow-Headers': 'Content-Type, X-Installation-Id',
};

function json(status: number, data: unknown): Response {
  if (status === 204) {
    return new Response(null, { status, headers: CORS_HEADERS });
  }
  return Response.json(data, { status, headers: CORS_HEADERS });
}

function notFound(message = 'Ruta no encontrada.'): Response {
  return json(404, { message });
}

function intParam(value: string | null | undefined): number | null {
  if (value === undefined || value === null) {
    return null;
  }
  const parsed = Number.parseInt(value, 10);
  return Number.isFinite(parsed) ? parsed : null;
}

export default async function handler(req: Request): Promise<Response> {
  if (req.method === 'OPTIONS') {
    return json(204, null);
  }

  const url = new URL(req.url);
  const path = url.pathname;
  const method = req.method;
  const query = url.searchParams;
  const installationId = req.headers.get('x-installation-id');

  try {
    const store = await getDataStore();
    const api = new Api(store);
    let body: Record<string, unknown> = {};
    if (method !== 'GET') {
      body = (await req.json().catch(() => ({}))) as Record<string, unknown>;
    }

    if (method === 'GET' && path === '/api/health') {
      return json(200, { status: 'healthy' });
    }

    const listMatch = path.match(/^\/api\/(conductores|vehiculos|rutas)$/);
    if (listMatch) {
      const collection = listMatch[1];
      if (method === 'GET') {
        const data = collection === 'conductores'
          ? await api.listConductores()
          : collection === 'vehiculos'
            ? await api.listVehiculos()
            : await api.listRutas();
        return json(200, data);
      }
      if (method === 'POST') {
        const data = collection === 'conductores'
          ? await api.createConductor(body)
          : collection === 'vehiculos'
            ? await api.createVehiculo(body)
            : await api.createRuta(body);
        return json(201, data);
      }
    }

    const itemMatch = path.match(/^\/api\/(conductores|vehiculos|rutas)\/(\d+)$/);
    if (itemMatch) {
      const collection = itemMatch[1];
      const id = Number.parseInt(itemMatch[2], 10);
      if (method === 'GET') {
        const data = collection === 'conductores'
          ? await api.getConductor(id)
          : collection === 'vehiculos'
            ? await api.getVehiculo(id)
            : await api.getRuta(id);
        return data ? json(200, data) : json(404, { message: 'Registro no encontrado.' });
      }
      if (method === 'PUT') {
        if (collection === 'conductores') {
          await api.updateConductor(id, body);
        } else if (collection === 'vehiculos') {
          await api.updateVehiculo(id, body);
        } else {
          await api.updateRuta(id, body);
        }
        return json(204, null);
      }
      if (method === 'DELETE') {
        if (collection === 'conductores') {
          await api.deleteConductor(id);
        } else if (collection === 'vehiculos') {
          await api.deleteVehiculo(id);
        } else {
          await api.deleteRuta(id);
        }
        return json(204, null);
      }
    }

    if (method === 'POST' && path === '/api/tracking/devices/pair') {
      return json(200, await api.pairDevice({
        ...body,
        installationId: (body.installationId as string | undefined) ?? installationId ?? undefined,
      }));
    }

    if (method === 'POST' && path === '/api/tracking/sessions') {
      return json(200, await api.startSession(body, installationId));
    }

    const stopMatch = path.match(/^\/api\/tracking\/sessions\/(\d+)\/stop$/);
    if (stopMatch && method === 'POST') {
      const conductorId = intParam(query.get('conductorId'));
      if (conductorId === null) {
        return json(400, { message: 'El conductor es obligatorio.' });
      }
      return json(200, await api.stopSession(Number.parseInt(stopMatch[1], 10), conductorId));
    }

    const conductorRoute = path.match(/^\/api\/tracking\/conductores\/(\d+)\/(active-session|latest|history)$/);
    if (conductorRoute && method === 'GET') {
      const conductorId = Number.parseInt(conductorRoute[1], 10);
      if (conductorRoute[2] === 'active-session') {
        return json(200, await api.getActiveSession(conductorId));
      }
      if (conductorRoute[2] === 'latest') {
        return json(200, await api.getLatest(conductorId));
      }
      const limit = intParam(query.get('limit')) ?? 200;
      return json(200, await api.getHistory(conductorId, limit));
    }

    if (method === 'POST' && path === '/api/tracking/pings') {
      return json(200, await api.addPing(body, installationId));
    }

    if (method === 'POST' && path === '/api/tracking/pings/batch') {
      return json(200, await api.addPingsBatch(body, installationId));
    }

    if (method === 'GET' && path === '/api/tracking/live') {
      return json(200, await api.getLive());
    }

    return notFound();
  } catch (error) {
    if (error instanceof ApiError) {
      return json(error.status, { message: error.message });
    }
    console.error('API error:', error);
    return json(500, { message: 'Error interno del servidor.' });
  }
}