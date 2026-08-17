import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { trackingApi } from './api';
import type { DriverLocation } from './types';
import { startTrackingHub, stopTrackingHub } from './trackingHub';

export type RealtimeSource = 'signalr' | 'polling' | 'off';

export function useLiveTracking(enabled: boolean) {
  const [locations, setLocations] = useState<DriverLocation[]>([]);
  const [source, setSource] = useState<RealtimeSource>('off');
  const [lastUpdated, setLastUpdated] = useState<number | null>(null);
  const mounted = useRef(true);

  useEffect(() => {
    mounted.current = true;
    return () => {
      mounted.current = false;
    };
  }, []);

  useEffect(() => {
    if (!enabled) {
      setSource('off');
      setLocations([]);
      return;
    }

    let pollId: number | null = null;
    let disposed = false;

    const mergeLocation = (loc: DriverLocation) => {
      setLocations((prev) => {
        const exists = prev.some((l) => l.conductorId === loc.conductorId);
        const next = exists
          ? prev.map((l) => (l.conductorId === loc.conductorId ? loc : l))
          : [...prev, loc];
        return next.sort((a, b) => b.recordedAtUtc.localeCompare(a.recordedAtUtc));
      });
      setSource('signalr');
      setLastUpdated(Date.now());
    };

    const poll = async () => {
      if (disposed || !mounted.current) return;
      try {
        const data = await trackingApi.getLive();
        if (disposed || !mounted.current) return;
        setLocations(data);
        setLastUpdated(Date.now());
      } catch {
        // keep last data
      }
    };

    const startPolling = () => {
      if (pollId !== null || disposed) return;
      setSource('polling');
      void poll();
      pollId = window.setInterval(poll, 10000);
    };

    const begin = async () => {
      let hub: signalR.HubConnection | null = null;
      try {
        hub = await startTrackingHub(mergeLocation);
      } catch {
        hub = null;
      }
      if (disposed) {
        if (hub) void stopTrackingHub();
        return;
      }
      if (!hub) {
        startPolling();
        return;
      }
      try {
        const data = await hub.invoke<DriverLocation[]>('GetLive');
        if (disposed || !mounted.current) return;
        if (data && data.length > 0) {
          setLocations(data);
          setSource('signalr');
          setLastUpdated(Date.now());
        }
      } catch {
        if (!disposed) startPolling();
      }
    };

    void begin();

    const watchdog = window.setTimeout(() => {
      if (mounted.current && !isHubConfigured()) startPolling();
    }, 8000);

    return () => {
      disposed = true;
      window.clearTimeout(watchdog);
      if (pollId !== null) window.clearInterval(pollId);
      void stopTrackingHub();
    };
  }, [enabled]);

  return { locations, source, lastUpdated };
}

function isHubConfigured(): boolean {
  return Boolean(import.meta.env.VITE_SIGNALR_URL);
}