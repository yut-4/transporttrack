import type { LocationSample, LocationSource, DriverPosition } from './types';

export interface LocationProvider {
  source: LocationSource;
  isSupported(): boolean;
  getCurrentPosition(): Promise<DriverPosition>;
  watchPosition(
    onPosition: (pos: DriverPosition) => void,
    onError: (err: Error) => void,
  ): () => void;
}

export function toLocationSample(pos: DriverPosition, source: LocationSource): LocationSample {
  return {
    latitude: pos.latitude,
    longitude: pos.longitude,
    accuracy: pos.accuracy,
    altitude: pos.altitude,
    altitudeAccuracy: null,
    speed: pos.speed,
    heading: pos.heading,
    timestamp: pos.timestamp,
    source,
  };
}

export class BrowserLocationProvider implements LocationProvider {
  source: LocationSource = 'web';

  isSupported(): boolean {
    return typeof navigator !== 'undefined' && 'geolocation' in navigator;
  }

  getCurrentPosition(): Promise<DriverPosition> {
    return new Promise((resolve, reject) => {
      if (!this.isSupported()) {
        reject(new Error('Geolocalización no soportada por este navegador'));
        return;
      }
      navigator.geolocation.getCurrentPosition(
        (position) => resolve(mapPosition(position)),
        (err) => reject(new Error(err.message)),
        { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 },
      );
    });
  }

  watchPosition(
    onPosition: (pos: DriverPosition) => void,
    onError: (err: Error) => void,
  ): () => void {
    if (!this.isSupported()) {
      onError(new Error('Geolocalización no soportada por este navegador'));
      return () => undefined;
    }
    const watchId = navigator.geolocation.watchPosition(
      (position) => onPosition(mapPosition(position)),
      (err) => onError(new Error(err.message)),
      { enableHighAccuracy: true, timeout: 20000, maximumAge: 5000 },
    );
    return () => navigator.geolocation.clearWatch(watchId);
  }
}

function mapPosition(position: GeolocationPosition): DriverPosition {
  const c = position.coords;
  return {
    latitude: c.latitude,
    longitude: c.longitude,
    accuracy: c.accuracy ?? null,
    altitude: c.altitude ?? null,
    speed: c.speed ?? null,
    heading: c.heading ?? null,
    timestamp: position.timestamp,
  };
}

export async function createCapacitorLocationProvider(): Promise<LocationProvider | null> {
  try {
    const { Capacitor } = await import('@capacitor/core');
    const { Geolocation } = await import('@capacitor/geolocation');
    if (!Capacitor.isNativePlatform()) return null;
    return new CapacitorLocationProvider(Geolocation, Capacitor.getPlatform());
  } catch {
    return null;
  }
}

export async function getBestLocationProvider(): Promise<LocationProvider> {
  const capacitor = await createCapacitorLocationProvider();
  if (capacitor) {
    return capacitor;
  }
  return new BrowserLocationProvider();
}

type CapacitorGeolocationLike = {
  getCurrentPosition(options?: unknown): Promise<{ coords: Record<string, unknown>; timestamp: number }>;
  watchPosition(options: unknown, callback: (position: { coords: Record<string, unknown>; timestamp: number }, err?: unknown) => void): Promise<string>;
  clearWatch(options: { id: string }): Promise<void>;
};

class CapacitorLocationProvider implements LocationProvider {
  source: LocationSource;

  constructor(
    private readonly geo: CapacitorGeolocationLike,
    platform: string,
  ) {
    this.source = platform === 'ios' ? 'ios' : 'android';
  }

  isSupported(): boolean {
    return true;
  }

  async getCurrentPosition(): Promise<DriverPosition> {
    const pos = await this.geo.getCurrentPosition({
      enableHighAccuracy: true,
      timeout: 15000,
    });
    return mapCapacitorPosition(pos);
  }

  watchPosition(
    onPosition: (pos: DriverPosition) => void,
    onError: (err: Error) => void,
  ): () => void {
    let id: string | null = null;
    this.geo
      .watchPosition({ enableHighAccuracy: true, maximumAge: 5000 }, (position, err) => {
        if (err) {
          onError(new Error(String(err)));
          return;
        }
        onPosition(mapCapacitorPosition(position));
      })
      .then((watchId) => {
        id = watchId;
      })
      .catch((e: unknown) => onError(e instanceof Error ? e : new Error(String(e))));
    return () => {
      if (id) {
        void this.geo.clearWatch({ id });
      }
    };
  }
}

function mapCapacitorPosition(position: {
  coords: Record<string, unknown>;
  timestamp: number;
}): DriverPosition {
  const c = position.coords;
  const num = (v: unknown): number | null => (typeof v === 'number' ? v : null);
  return {
    latitude: c.latitude as number,
    longitude: c.longitude as number,
    accuracy: num(c.accuracy),
    altitude: num(c.altitude),
    speed: num(c.speed),
    heading: num(c.heading),
    timestamp: typeof position.timestamp === 'number' ? position.timestamp : Date.now(),
  };
}