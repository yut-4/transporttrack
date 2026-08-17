import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { conductorApi, vehiculoApi, rutaApi } from '../../api/client';
import { demoStore } from '../../api/demoStore';
import type { ConductorDto, VehiculoDto, RutaDto } from '../../types';
import { trackingApi } from './api';
import { useDriverTracking } from './useDriverTracking';

const INSTALLATION_KEY = 'transporttrack-installation-id';
const PAIRED_CONDUCTOR_KEY = 'transporttrack-paired-conductor';

interface PairedInfo {
  conductorId: number;
  conductorNombre: string;
}

function getInstallationId(): string {
  let id = window.localStorage.getItem(INSTALLATION_KEY);
  if (!id) {
    id = crypto.randomUUID();
    window.localStorage.setItem(INSTALLATION_KEY, id);
  }
  return id;
}

function getPairedConductor(): PairedInfo | null {
  const raw = window.localStorage.getItem(PAIRED_CONDUCTOR_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as PairedInfo;
  } catch {
    return null;
  }
}

export default function DriverTrackingPage() {
  const installationId = useMemo(getInstallationId, []);
  const [conductores, setConductores] = useState<ConductorDto[]>([]);
  const [vehiculos, setVehiculos] = useState<VehiculoDto[]>([]);
  const [rutas, setRutas] = useState<RutaDto[]>([]);
  const [paired, setPaired] = useState<PairedInfo | null>(getPairedConductor);
  const [vehiculoId, setVehiculoId] = useState<number | null>(null);
  const [rutaId, setRutaId] = useState<number | null>(null);
  const [pairError, setPairError] = useState<string | null>(null);

  const tracking = useDriverTracking({
    installationId,
    conductorId: paired?.conductorId ?? null,
    vehiculoId,
    rutaId,
  });

  useEffect(() => {
    (async () => {
      try {
        const [cons, vhs, rt] = await Promise.all([
          conductorApi.getAll(),
          vehiculoApi.getAll(),
          rutaApi.getAll(),
        ]);
        setConductores(cons);
        setVehiculos(vhs);
        setRutas(rt);
      } catch {
        setConductores(demoStore.getConductores());
        setVehiculos(demoStore.getVehiculos());
        setRutas(demoStore.getRutas());
      }
    })();
  }, []);

  const availableVehiculos = useMemo(
    () => vehiculos.filter((v) => v.conductorId === paired?.conductorId),
    [vehiculos, paired],
  );
  const availableRutas = useMemo(
    () => rutas.filter((r) => r.conductorId === paired?.conductorId),
    [rutas, paired],
  );

  const handlePair = async (conductorId: number) => {
    setPairError(null);
    try {
      await trackingApi.pairDevice({
        installationId,
        platform: 'web',
        name: 'Navegador',
        conductorId,
      });
      const info = { conductorId, conductorNombre: conductores.find((c) => c.id === conductorId)?.nombre ?? '' };
      window.localStorage.setItem(PAIRED_CONDUCTOR_KEY, JSON.stringify(info));
      setPaired(info);
    } catch (e: unknown) {
      setPairError(e instanceof Error ? e.message : 'No se pudo emparejar el dispositivo.');
    }
  };

  const handleUnpair = () => {
    window.localStorage.removeItem(PAIRED_CONDUCTOR_KEY);
    setPaired(null);
    setVehiculoId(null);
    setRutaId(null);
  };

  const speedKmh =
    tracking.live?.speed !== null && tracking.live?.speed !== undefined
      ? Math.round(tracking.live.speed * 3.6)
      : null;

  return (
    <div className="app-container" style={{ maxWidth: 760 }}>
      <header className="header">
        <h1>
          <i className="fas fa-location-dot"></i> Seguimiento del conductor
        </h1>
        <div className="header-actions">
          <Link to="/" className="btn btn-sm">
            <i className="fas fa-arrow-left"></i> Panel admin
          </Link>
        </div>
      </header>

      <div className="tracking-layout">
        {tracking.status === 'tracking' && (
          <div className="gps-banner active">
            <i className="fas fa-circle" style={{ fontSize: '0.5rem' }}></i> GPS activo: compartiendo ubicación en tiempo real
          </div>
        )}
        {tracking.error && (
          <div className="gps-banner error">
            <i className="fas fa-triangle-exclamation"></i> {tracking.error}
          </div>
        )}

        {!paired ? (
          <div className="tracking-card">
            <h3>Emparejar dispositivo</h3>
            <p style={{ color: 'var(--muted)', fontSize: '0.88rem', marginBottom: 14 }}>
              Selecciona tu cuenta de conductor para vincular este dispositivo y comenzar a compartir tu ubicación.
            </p>
            <div className="tracking-field">
              <label>Conductor</label>
              <select
                value=""
                onChange={(e) => {
                  const id = Number(e.target.value);
                  if (id) void handlePair(id);
                }}
              >
                <option value="">Selecciona tu cuenta</option>
                {conductores.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nombre} · {c.licencia}
                  </option>
                ))}
              </select>
            </div>
            {pairError && (
              <p style={{ color: 'var(--danger)', fontSize: '0.85rem', marginTop: 10 }}>{pairError}</p>
            )}
          </div>
        ) : (
          <div className="tracking-card">
            <h3>
              Conductor: {paired.conductorNombre}
              <button className="btn btn-sm btn-warning" style={{ marginLeft: 10 }} onClick={handleUnpair}>
                Cambiar
              </button>
            </h3>

            {tracking.status !== 'tracking' && (
              <>
                <div className="tracking-field">
                  <label>Vehículo (opcional)</label>
                  <select value={vehiculoId ?? ''} onChange={(e) => setVehiculoId(e.target.value ? Number(e.target.value) : null)}>
                    <option value="">Sin vehículo asignado</option>
                    {availableVehiculos.map((v) => (
                      <option key={v.id} value={v.id}>
                        {v.placa} · {v.marca} {v.modelo}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="tracking-field">
                  <label>Ruta (opcional)</label>
                  <select value={rutaId ?? ''} onChange={(e) => setRutaId(e.target.value ? Number(e.target.value) : null)}>
                    <option value="">Sin ruta asignada</option>
                    {availableRutas.map((r) => (
                      <option key={r.id} value={r.id}>
                        {r.origen} → {r.destino}
                      </option>
                    ))}
                  </select>
                </div>
                <div style={{ marginTop: 18, display: 'flex', gap: 10, flexWrap: 'wrap' }}>
                  <button className="btn btn-primary" onClick={() => void tracking.startTracking()} disabled={tracking.status === 'error'}>
                    <i className="fas fa-play"></i> Iniciar seguimiento
                  </button>
                  {tracking.simulatorEnabled && (
                    <span className="badge">
                      <i className="fas fa-robot"></i> Modo SIMULACIÓN (GPS simulado)
                    </span>
                  )}
                </div>
              </>
            )}

            {tracking.status === 'tracking' && (
              <div className="tracking-active">
                <div className="tracking-speed">
                  {speedKmh !== null ? speedKmh : 0}
                  <small> km/h</small>
                </div>
                <div style={{ color: 'var(--muted)', fontSize: '0.82rem', marginTop: 4 }}>
                  {tracking.elapsedSec > 0 && (
                    <span className="tracking-timer">
                      {Math.floor(tracking.elapsedSec / 60)}:{(tracking.elapsedSec % 60).toString().padStart(2, '0')} ·{' '}
                    </span>
                  )}
                  Precisión: {tracking.live?.accuracy !== null && tracking.live?.accuracy !== undefined ? `±${Math.round(tracking.live.accuracy)} m` : '—'}
                </div>

                <div className="tracking-detail-grid">
                  <div className="tracking-detail">
                    <div className="k">Vehículo</div>
                    <div className="v">{vehiculos.find((v) => v.id === vehiculoId)?.placa ?? '—'}</div>
                  </div>
                  <div className="tracking-detail">
                    <div className="k">Ruta</div>
                    <div className="v">
                      {(() => {
                        const r = rutas.find((x) => x.id === rutaId);
                        return r ? `${r.origen} → ${r.destino}` : '—';
                      })()}
                    </div>
                  </div>
                  <div className="tracking-detail">
                    <div className="k">Pendientes en outbox</div>
                    <div className="v">{tracking.pendingOutbox}</div>
                  </div>
                  <div className="tracking-detail">
                    <div className="k">Fuente</div>
                    <div className="v">{tracking.provider?.source === 'web' ? 'Web (GPS del navegador)' : tracking.provider?.source === 'ios' ? 'iOS' : 'Android'}</div>
                  </div>
                </div>

                <button className="btn btn-danger" onClick={() => void tracking.stopTracking()}>
                  <i className="fas fa-stop"></i> Finalizar seguimiento
                </button>
              </div>
            )}
          </div>
        )}

        <div style={{ color: 'var(--muted)', fontSize: '0.8rem', textAlign: 'center' }}>
          <i className="fas fa-shield-halved"></i> La ubicación solo se comparte mientras el seguimiento esté activo. Se requiere HTTPS.
        </div>
      </div>
    </div>
  );
}