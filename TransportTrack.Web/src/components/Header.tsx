import { type StatInfo } from '../types';

interface HeaderProps {
  stats: StatInfo[];
}

export default function Header({ stats }: HeaderProps) {
  return (
    <>
      <header className="header">
        <h1>
          <i className="fas fa-truck-fast"></i> TransportTrack
        </h1>
        <span className="badge">
          <i className="fas fa-database"></i> Gestión de flotas
        </span>
      </header>

      <div className="stats">
        {stats.map((stat) => (
          <div className="stat-card" key={stat.label}>
            <div className="label">
              <i className={stat.icon}></i> {stat.label}
            </div>
            <div className="value">
              <i className="fas fa-hashtag"></i> {stat.value}
            </div>
          </div>
        ))}
      </div>
    </>
  );
}
