import type {
  DriverLocation,
  TrackingPingRequest,
  TrackingSessionInfo,
} from './types';

const base = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? '';
const API_URL = base + '/api';

let currentInstallationId: string | null = null;

export function setTrackingInstallationId(id: string): void {
  currentInstallationId = id;
}

export interface PairDeviceRequest {
  installationId: string;
  platform: string;
  name: string | null;
  conductorId: number;
}

export interface PairDeviceResponse {
  deviceId: number;
  conductorId: number;
  conductorNombre: string;
}

export interface StartSessionRequest {
  conductorId: number;
  vehiculoId?: number | null;
  rutaId?: number | null;
  deviceId?: number;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  headers.set('Content-Type', 'application/json');
  if (currentInstallationId) {
    headers.set('X-Installation-Id', currentInstallationId);
  }
  const res = await fetch(API_URL + path, {
    ...init,
    headers,
  });
  if (!res.ok) {
    let detail = `Error ${res.status}`;
    try {
      const body = (await res.json()) as { message?: string; title?: string; detail?: string };
      detail = body.detail || body.title || body.message || detail;
    } catch {
      // ignore parse errors
    }
    throw new Error(detail);
  }
  if (res.status === 204) {
    return undefined as T;
  }
  return res.json() as Promise<T>;
}

export const trackingApi = {
  pairDevice(req: PairDeviceRequest): Promise<PairDeviceResponse> {
    return request('/tracking/devices/pair', {
      method: 'POST',
      body: JSON.stringify(req),
    });
  },

  startSession(req: StartSessionRequest): Promise<TrackingSessionInfo> {
    return request('/tracking/sessions', {
      method: 'POST',
      body: JSON.stringify(req),
    });
  },

  stopSession(sessionId: number, conductorId: number): Promise<TrackingSessionInfo> {
    return request(`/tracking/sessions/${sessionId}/stop?conductorId=${conductorId}`, { method: 'POST' });
  },

  sendPing(ping: TrackingPingRequest): Promise<{ accepted: boolean }> {
    return request('/tracking/pings', {
      method: 'POST',
      body: JSON.stringify(ping),
    });
  },

  sendBatchPings(pings: TrackingPingRequest[]): Promise<{ accepted: number }> {
    return request('/tracking/pings/batch', {
      method: 'POST',
      body: JSON.stringify({ pings }),
    });
  },

  getLive(): Promise<DriverLocation[]> {
    return request('/tracking/live');
  },

  getLatest(conductorId: number): Promise<DriverLocation | null> {
    return request(`/tracking/conductores/${conductorId}/latest`);
  },

  getHistory(conductorId: number, limit = 200): Promise<DriverLocation[]> {
    return request(`/tracking/conductores/${conductorId}/history?limit=${limit}`);
  },

  getActiveSession(conductorId: number): Promise<TrackingSessionInfo | null> {
    return request(`/tracking/conductores/${conductorId}/active-session`);
  },
};