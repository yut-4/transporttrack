import { useCallback, useEffect, useRef, useState } from 'react';
import { setTrackingInstallationId, trackingApi } from './api';
import {
  getBestLocationProvider,
  toLocationSample,
  type LocationProvider,
} from './locationProvider';
import { countOutbox, enqueuePings, peekPings, removePings } from './outbox';
import type { DriverPosition, LocationSample, LocationSource, TrackingPingRequest, TrackingSessionInfo } from './types';

export type TrackingStatus = 'idle' | 'tracking' | 'error';

interface UseDriverTrackingOptions {
  installationId: string;
  conductorId: number | null;
  vehiculoId: number | null;
  rutaId: number | null;
}

const BATCH_MAX = 100;
const MIN_UPLOAD_INTERVAL_MS = 10_000;
const MOVEMENT_THRESHOLD_M = 15;
const HEARTBEAT_MS = 60_000;
const OUTBOX_FLUSH_MS = 15_000;

export function isGpsSimulatorEnabled(): boolean {
  return import.meta.env.VITE_ENABLE_GPS_SIMULATOR === 'true';
}

export function useDriverTracking({ installationId, conductorId, vehiculoId, rutaId }: UseDriverTrackingOptions) {
  const [status, setStatus] = useState<TrackingStatus>('idle');
  const [session, setSession] = useState<TrackingSessionInfo | null>(null);
  const [provider, setProvider] = useState<LocationProvider | null>(null);
  const [live, setLive] = useState<LocationSample | null>(null);
  const [pendingOutbox, setPendingOutbox] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [elapsedSec, setElapsedSec] = useState(0);

  const conductorIdRef = useRef<number | null>(conductorId);
  conductorIdRef.current = conductorId;
  const vehiculoIdRef = useRef<number | null>(vehiculoId);
  vehiculoIdRef.current = vehiculoId;
  const rutaIdRef = useRef<number | null>(rutaId);
  rutaIdRef.current = rutaId;
  const sessionRef = useRef<TrackingSessionInfo | null>(session);
  sessionRef.current = session;

  const lastUploadRef = useRef<{ timestamp: number; latitude: number; longitude: number } | null>(null);
  const startedAtRef = useRef<number | null>(null);
  const stopWatchRef = useRef<(() => void) | null>(null);
  const simulatorIdRef = useRef<number | null>(null);
  const timerRef = useRef<number | null>(null);
  const sourceRef = useRef<LocationSource>('web');

  useEffect(() => {
    setTrackingInstallationId(installationId);
  }, [installationId]);

  const refreshOutboxCount = useCallback(async () => {
    setPendingOutbox(await countOutbox());
  }, []);

  const flushOutbox = useCallback(async () => {
    const pings = await peekPings(BATCH_MAX);
    if (pings.length === 0) {
      await refreshOutboxCount();
      return;
    }
    try {
      const result = await trackingApi.sendBatchPings(pings);
      if (result.accepted > 0) {
        await removePings(pings.map((p) => p.clientPingId));
      }
    } catch {
      // keep in outbox, retry later
    }
    await refreshOutboxCount();
  }, [refreshOutboxCount]);

  const enqueueSample = useCallback(
    async (sample: LocationSample) => {
      const active = sessionRef.current;
      if (!active) return;
      const ping: TrackingPingRequest = {
        clientPingId: crypto.randomUUID(),
        sessionId: active.sessionId,
        latitude: sample.latitude,
        longitude: sample.longitude,
        accuracyMeters: sample.accuracy,
        altitudeMeters: sample.altitude,
        speedMetersPerSecond: sample.speed,
        headingDegrees: sample.heading,
        recordedAtUtc: new Date(sample.timestamp).toISOString(),
        source: sample.source,
      };
      await enqueuePings([ping]);
      await flushOutbox();
    },
    [flushOutbox],
  );

  const handlePosition = useCallback(
    (pos: DriverPosition) => {
      const now = pos.timestamp;
      const last = lastUploadRef.current;
      const elapsed = last ? now - last.timestamp : Number.MAX_SAFE_INTEGER;

      let moved = false;
      if (last) {
        const dLat = (pos.latitude - last.latitude) * 111_320;
        const dLng = (pos.longitude - last.longitude) * 111_320 * Math.cos((pos.latitude * Math.PI) / 180);
        moved = Math.sqrt(dLat * dLat + dLng * dLng) >= MOVEMENT_THRESHOLD_M;
      } else {
        moved = true;
      }

      if (elapsed >= MIN_UPLOAD_INTERVAL_MS && (moved || elapsed >= HEARTBEAT_MS)) {
        lastUploadRef.current = {
          timestamp: now,
          latitude: pos.latitude,
          longitude: pos.longitude,
        };
        void enqueueSample(toLocationSample(pos, sourceRef.current));
      }
    },
    [enqueueSample],
  );

  const startTimer = useCallback(() => {
    startedAtRef.current = Date.now();
    timerRef.current = window.setInterval(() => {
      if (startedAtRef.current) {
        setElapsedSec(Math.floor((Date.now() - startedAtRef.current) / 1000));
      }
    }, 1000);
  }, []);

  const startTracking = useCallback(async () => {
    const active = conductorIdRef.current;
    if (!active) return;

    setError(null);
    try {
      let activeSession = sessionRef.current;
      if (!activeSession) {
        activeSession = await trackingApi.startSession({
          conductorId: active,
          vehiculoId: vehiculoIdRef.current,
          rutaId: rutaIdRef.current,
        });
        setSession(activeSession);
        sessionRef.current = activeSession;
      }

      if (isGpsSimulatorEnabled()) {
        sourceRef.current = 'web';
        setStatus('tracking');
        startTimer();
        startSimulatorLoop(handlePosition, setLive, simulatorIdRef);
        return;
      }

      const prov = await getBestLocationProvider();
      if (!prov.isSupported()) {
        setError('Este navegador no soporta geolocalización. Usa HTTPS o la app móvil.');
        setStatus('error');
        return;
      }
      setProvider(prov);
      sourceRef.current = prov.source;
      setStatus('tracking');
      startTimer();

      stopWatchRef.current = prov.watchPosition(
        (pos) => {
          setLive(toLocationSample(pos, prov.source));
          handlePosition(pos);
        },
        (err) => setError(err.message),
      );
    } catch (e: unknown) {
      setStatus('error');
      setError(e instanceof Error ? e.message : 'No se pudo iniciar el seguimiento.');
    }
  }, [handlePosition, startTimer]);

  const stopTracking = useCallback(async () => {
    stopWatchRef.current?.();
    stopWatchRef.current = null;
    if (simulatorIdRef.current !== null) {
      window.clearInterval(simulatorIdRef.current);
      simulatorIdRef.current = null;
    }
    if (timerRef.current !== null) {
      window.clearInterval(timerRef.current);
      timerRef.current = null;
    }
    await flushOutbox();
    const active = sessionRef.current;
    if (active) {
      try {
        const stopped = await trackingApi.stopSession(active.sessionId, active.conductorId);
        setSession(stopped);
      } catch (e: unknown) {
        setError(e instanceof Error ? e.message : 'No se pudo finalizar la sesión.');
      }
    }
    setStatus('idle');
    lastUploadRef.current = null;
    startedAtRef.current = null;
    setLive(null);
  }, [flushOutbox]);

  useEffect(() => {
    const outboxInterval = window.setInterval(() => {
      void flushOutbox();
    }, OUTBOX_FLUSH_MS);
    void refreshOutboxCount();
    return () => window.clearInterval(outboxInterval);
  }, [flushOutbox, refreshOutboxCount]);

  useEffect(() => {
    return () => {
      stopWatchRef.current?.();
      if (simulatorIdRef.current !== null) {
        window.clearInterval(simulatorIdRef.current);
      }
      if (timerRef.current !== null) {
        window.clearInterval(timerRef.current);
      }
    };
  }, []);

  return {
    status,
    session,
    live,
    provider,
    error,
    pendingOutbox,
    elapsedSec,
    simulatorEnabled: isGpsSimulatorEnabled(),
    startTracking,
    stopTracking,
  };
}

function startSimulatorLoop(
  handlePosition: (pos: DriverPosition) => void,
  setLive: (s: LocationSample | null) => void,
  idRef: { current: number | null },
): void {
  const baseLat = -12.0464;
  const baseLng = -77.0428;
  let step = 0;
  idRef.current = window.setInterval(() => {
    step += 1;
    const pos: DriverPosition = {
      latitude: baseLat + step * 0.0006,
      longitude: baseLng + step * 0.0006,
      accuracy: 8,
      altitude: 154,
      speed: 12.5,
      heading: 45,
      timestamp: Date.now(),
    };
    setLive(toLocationSample(pos, 'web'));
    handlePosition(pos);
  }, 5000);
}