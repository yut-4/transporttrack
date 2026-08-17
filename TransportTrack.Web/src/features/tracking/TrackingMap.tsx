import { useEffect, useRef } from 'react';
import * as maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import type { DriverLocation } from './types';

interface TrackingMapProps {
  locations: DriverLocation[];
  selectedId: number | null;
  focusPoint?: { latitude: number; longitude: number } | null;
}

const DEFAULT_STYLE =
  'https://demotiles.maplibre.org/style.json';

function mapStyle(): string {
  return (import.meta.env.VITE_MAP_STYLE_URL as string | undefined) || DEFAULT_STYLE;
}

export default function TrackingMap({ locations, selectedId, focusPoint }: TrackingMapProps) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<maplibregl.Map | null>(null);
  const markersRef = useRef<Map<number, maplibregl.Marker>>(new Map());
  const selectedRef = useRef<number | null>(selectedId);
  selectedRef.current = selectedId;

  useEffect(() => {
    if (!containerRef.current) return;
    const map = new maplibregl.Map({
      container: containerRef.current,
      style: mapStyle(),
      center: [-69.9153, 18.5041],
      zoom: 5,
      attributionControl: false,
    });
    map.addControl(new maplibregl.NavigationControl({ showCompass: false }), 'top-right');
    mapRef.current = map;
    return () => {
      map.remove();
      mapRef.current = null;
      markersRef.current.clear();
    };
  }, []);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;

    const seen = new Set<number>();

    for (const loc of locations) {
      seen.add(loc.conductorId);
      const existing = markersRef.current.get(loc.conductorId);
      const el = document.createElement('div');
      el.style.width = '26px';
      el.style.height = '26px';
      el.style.borderRadius = '50% 50% 50% 0';
      el.style.background = loc.conductorId === selectedRef.current ? 'var(--primary)' : '#2a7de1';
      el.style.transform = 'rotate(-45deg)';
      el.style.border = '3px solid var(--surface)';
      el.style.boxShadow = '0 4px 14px rgba(0,0,0,0.25)';
      const dot = document.createElement('span');
      dot.style.position = 'absolute';
      dot.style.width = '10px';
      dot.style.height = '10px';
      dot.style.borderRadius = '50%';
      dot.style.background = '#fff';
      dot.style.left = '5px';
      dot.style.top = '5px';
      dot.style.transform = 'rotate(45deg)';
      el.appendChild(dot);

      if (existing) {
        existing.getElement().replaceWith(el);
        existing.remove();
        markersRef.current.delete(loc.conductorId);
      }

      const marker = new maplibregl.Marker({ element: el, anchor: 'center' })
        .setLngLat([loc.longitude, loc.latitude])
        .setPopup(
          new maplibregl.Popup({ offset: 22, closeButton: false })
            .setHTML(
              `<strong>${escapeHtml(loc.conductorNombre)}</strong>` +
                `<div style="font-size:0.78rem;color:#5e6f82">${escapeHtml(loc.placa ?? '—')}</div>` +
                `<div style="font-size:0.78rem;color:#5e6f82">${speedLabel(loc.speedMetersPerSecond)}</div>`,
            ),
        )
        .addTo(map);
      markersRef.current.set(loc.conductorId, marker);
    }

    for (const [id, marker] of markersRef.current) {
      if (!seen.has(id)) {
        marker.remove();
        markersRef.current.delete(id);
      }
    }
  }, [locations]);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    const target = focusPoint ?? locations.find((l) => l.conductorId === selectedRef.current);
    if (target) {
      map.flyTo({ center: [target.longitude, target.latitude], zoom: Math.max(map.getZoom(), 12) });
    }
  }, [focusPoint, locations, selectedId]);

  const selected = locations.find((l) => l.conductorId === selectedRef.current) ?? locations[0] ?? null;

  return (
    <div
      ref={containerRef}
      className="gps-map"
      data-testid="gps-map"
      data-lat={selected?.latitude ?? ''}
      data-lng={selected?.longitude ?? ''}
    />
  );
}

function escapeHtml(value: string): string {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;');
}

function speedLabel(speed: number | null): string {
  if (speed === null || speed < 0) return 'Velocidad: —';
  return `Velocidad: ${Math.round(speed * 3.6)} km/h`;
}