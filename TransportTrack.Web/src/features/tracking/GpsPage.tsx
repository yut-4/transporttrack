import { useMemo, useState } from 'react';
import TrackingMap from './TrackingMap';
import { useLiveTracking } from './useLiveTracking';
import type { DriverLocation } from './types';

function relativeTime(iso: string | null): string {
  if (!iso) return '—';
  const ms = Date.now() - new Date(iso).getTime();
  if (ms < 0) return 'ahora';
  const sec = Math.floor(ms / 1000);
  if (sec < 60) return `hace ${sec} s`;
  const min = Math.floor(sec / 60);
  if (min < 60) return `hace ${min} min`;
  return `hace ${Math.floor(min / 60)} h`;
}

function stateDot(state: DriverLocation['state']): string {
  if (state === 'moving') return 'live-dot';
  if (state === 'stopped') return 'idle-dot';
  return 'offline-dot';
}

function stateLabel(state: DriverLocation['state']): { text: string; color: string } {
  switch (state) {
    case 'moving':
      return { text: 'En ruta', color: 'var(--success)' };
    case 'stopped':
      return { text: 'Detenido', color: 'var(--warning-text)' };
    case 'stale':
      return { text: 'Sin señal', color: 'var(--muted)' };
    default:
      return { text: 'Offline', color: 'var(--muted)' };
  }
}

export default function GpsPage() {
  const [enabled] = useState(true);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const { locations, source } = useLiveTracking(enabled);

  const selected = useMemo(
    () => locations.find((l) => l.conductorId === selectedId) ?? locations[0] ?? null,
    [locations, selectedId],
  );

  const kpis = useMemo(() => {
    const moving = locations.filter((l) => l.state === 'moving').length;
    const stopped = locations.filter((l) => l.state === 'stopped').length;
    const offline = locations.filter((l) => l.state === 'offline' || l.state === 'stale').length;
    return { moving, stopped, offline, total: locations.length };
  }, [locations]);

  return (
    <section className="section">
      <div className="section-header">
        <h2>
          <i className="fas fa-location-crosshairs"></i> GPS de conductores
        </h2>
        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
          <span className="badge">
            <span className={source === 'signalr' ? 'health-dot ok' : source === 'polling' ? 'health-dot warn' : 'health-dot bad'}></span>
            {source === 'signalr' ? 'Tiempo real (SignalR)' : source === 'polling' ? 'Actualización periódica' : 'Sin conexión'}
          </span>
        </div>
      </div>

      {kpis.total === 0 ? (
        <div
          style={{
            padding: '48px 24px',
            textAlign: 'center',
            color: 'var(--muted)',
            border: '1px solid var(--border)',
            borderRadius: 10,
          }}
        >
          <i className="fas fa-location-crosshairs" style={{ fontSize: '2rem', marginBottom: 12 }}></i>
          <p>
            Aún no hay conductores transmitiendo su ubicación.
          </p>
          <p style={{ marginTop: 8, fontSize: '0.85rem' }}>
            Un conductor debe abrir <strong>/tracking</strong> e iniciar el seguimiento para aparecer aquí.
          </p>
        </div>
      ) : (
        <div className="gps-layout">
          <aside className="driver-list">
            <div className="driver-list-header">
              Conductores · {kpis.total}
            </div>
            {locations.map((loc) => {
              const st = stateLabel(loc.state);
              return (
                <button
                  key={loc.conductorId}
                  className={`driver-item ${selected?.conductorId === loc.conductorId ? 'active' : ''}`}
                  onClick={() => setSelectedId(loc.conductorId)}
                >
                  <div className="driver-name">
                    <span>
                      <span className={stateDot(loc.state)}></span>
                      {loc.conductorNombre}
                    </span>
                    <span style={{ fontSize: '0.76rem', color: st.color }}>{st.text}</span>
                  </div>
                  <div className="driver-meta">
                    <span>{loc.placa ?? '—'}</span>
                    {loc.destino && (
                      <>
                        <span>•</span>
                        <span>→ {loc.destino}</span>
                      </>
                    )}
                    <span>•</span>
                    <span>{relativeTime(loc.recordedAtUtc)}</span>
                  </div>
                </button>
              );
            })}
          </aside>

          <div className="map-card">
            <div className="map-toolbar">
              <div>
                <div className="map-title">
                  {selected ? `${selected.conductorNombre}${selected.placa ? ` · ${selected.placa}` : ''}` : 'Selecciona un conductor'}
                </div>
                <div className="map-sub">
                  {selected
                    ? `${selected.origen ?? 'Origen'}${selected.destino ? ` → ${selected.destino}` : ''} · ubicación reciente`
                    : 'Ningún conductor seleccionado'}
                </div>
              </div>
              <div className="map-actions">
                <button
                  className="btn btn-sm"
                  onClick={() => setSelectedId(selected?.conductorId ?? null)}
                  title="Centrar en el conductor"
                >
                  <i className="fas fa-crosshairs"></i> Centrar
                </button>
              </div>
            </div>

            <TrackingMap
              locations={locations}
              selectedId={selected?.conductorId ?? null}
              focusPoint={selected ? { latitude: selected.latitude, longitude: selected.longitude } : null}
            />

            <div className="gps-kpis">
              <div className="gps-kpi">
                Velocidad
                <strong>
                  {selected && selected.speedMetersPerSecond !== null
                    ? `${Math.round(selected.speedMetersPerSecond * 3.6)} km/h`
                    : '—'}
                </strong>
              </div>
              <div className="gps-kpi">
                En ruta
                <strong>{kpis.moving}</strong>
              </div>
              <div className="gps-kpi">
                Detenidos
                <strong>{kpis.stopped}</strong>
              </div>
              <div className="gps-kpi">
                Último ping
                <strong>{selected ? relativeTime(selected.recordedAtUtc) : '—'}</strong>
              </div>
            </div>
          </div>
        </div>
      )}
    </section>
  );
}