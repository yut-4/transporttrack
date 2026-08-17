import { type StatInfo } from '../types';
import { useTheme } from '../theme/ThemeProvider';
import type { ThemeMode } from '../theme/theme';
import { Link } from 'react-router-dom';

interface HeaderProps {
  stats: StatInfo[];
  health?: {
    api: 'online' | 'offline' | 'demo';
    gps: 'active' | 'stopped';
    realtime: 'connected' | 'polling' | 'off';
  };
}

export default function Header({ stats, health }: HeaderProps) {
  const { mode, setMode } = useTheme();

  const apiLabel = health?.api === 'online' ? 'API' : health?.api === 'demo' ? 'Demo' : 'API off';

  return (
    <>
      <header className="header">
        <h1>
          <i className="fas fa-truck-fast"></i> TransportTrack
        </h1>
        <div className="header-actions">
          <Link to="/tracking" className="btn btn-sm">
            <i className="fas fa-location-dot"></i> Vista conductor
          </Link>
          {health && (
            <span className="badge">
              <i className="fas fa-circle" style={{ fontSize: '0.4rem', verticalAlign: 'middle', marginRight: 4, color: health.api === 'online' ? 'var(--live)' : health.api === 'demo' ? 'var(--idle)' : 'var(--danger)' }}></i>
              {apiLabel}
            </span>
          )}
          <select
            className="theme-select"
            aria-label="Tema"
            value={mode}
            onChange={(e) => setMode(e.target.value as ThemeMode)}
          >
            <option value="system">Sistema</option>
            <option value="light">Claro</option>
            <option value="dark">Oscuro</option>
          </select>
        </div>
      </header>

      <div className="stats">
        {stats.map((stat) => (
          <div className="stat-card" key={stat.label}>
            <div className="label">
              <i className={stat.icon}></i> {stat.label}
            </div>
            <div className="value">
              <span className="hash">
                <i className="fas fa-hashtag"></i>
              </span>
              {stat.value}
            </div>
          </div>
        ))}
      </div>
    </>
  );
}