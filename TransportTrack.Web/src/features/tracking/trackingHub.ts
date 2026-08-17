import * as signalR from '@microsoft/signalr';
import type { DriverLocation } from './types';

let hub: signalR.HubConnection | null = null;

export function isHubConfigured(): boolean {
  return Boolean(import.meta.env.VITE_SIGNALR_URL);
}

export async function startTrackingHub(
  onLocation: (loc: DriverLocation) => void,
): Promise<signalR.HubConnection | null> {
  const hubUrl = (import.meta.env.VITE_SIGNALR_URL as string | undefined)?.replace(/\/$/, '');
  if (!hubUrl) return null;

  if (hub) {
    hub.on('LocationUpdate', onLocation);
    return hub;
  }

  hub = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl + '/hubs/tracking')
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  try {
    await hub.start();
  } catch {
    hub = null;
    return null;
  }

  hub.on('LocationUpdate', onLocation);
  return hub;
}

export async function stopTrackingHub(): Promise<void> {
  if (hub) {
    await hub.stop().catch(() => undefined);
    hub = null;
  }
}