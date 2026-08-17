export type LocationSource = 'web' | 'android' | 'ios';

export interface LocationSample {
  latitude: number;
  longitude: number;
  accuracy: number | null;
  altitude: number | null;
  altitudeAccuracy: number | null;
  speed: number | null;
  heading: number | null;
  timestamp: number;
  source: LocationSource;
}

export interface TrackingPingRequest {
  clientPingId: string;
  sessionId: number;
  latitude: number;
  longitude: number;
  accuracyMeters: number | null;
  altitudeMeters: number | null;
  speedMetersPerSecond: number | null;
  headingDegrees: number | null;
  recordedAtUtc: string;
  source: LocationSource;
}

export type LiveState = 'moving' | 'stopped' | 'stale' | 'offline';

export interface DriverLocation {
  conductorId: number;
  conductorNombre: string;
  vehiculoId: number | null;
  placa: string | null;
  rutaId: number | null;
  origen: string | null;
  destino: string | null;
  latitude: number;
  longitude: number;
  accuracyMeters: number | null;
  speedMetersPerSecond: number | null;
  headingDegrees: number | null;
  recordedAtUtc: string;
  receivedAtUtc: string;
  state: LiveState;
}

export interface TrackingSessionInfo {
  sessionId: number;
  conductorId: number;
  conductorNombre: string;
  vehiculoId: number | null;
  placa: string | null;
  rutaId: number | null;
  origen: string | null;
  destino: string | null;
  status: 'active' | 'completed' | 'cancelled';
  startedAtUtc: string;
  deviceName: string | null;
}

export interface DriverPosition {
  latitude: number;
  longitude: number;
  accuracy: number | null;
  altitude: number | null;
  speed: number | null;
  heading: number | null;
  timestamp: number;
}