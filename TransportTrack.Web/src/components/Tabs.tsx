interface TabsProps {
  activeTab: string;
  setActiveTab: (tab: string) => void;
}

interface TabItem {
  id: string;
  label: string;
  icon: string;
  mobileLabel: string;
}

const tabs: TabItem[] = [
  { id: 'dashboard', label: 'Dashboard', icon: 'fas fa-gauge-high', mobileLabel: 'Inicio' },
  { id: 'conductores', label: 'Conductores', icon: 'fas fa-user-tie', mobileLabel: 'Conductores' },
  { id: 'vehiculos', label: 'Vehículos', icon: 'fas fa-truck', mobileLabel: 'Vehículos' },
  { id: 'rutas', label: 'Rutas', icon: 'fas fa-route', mobileLabel: 'Rutas' },
  { id: 'gps', label: 'GPS', icon: 'fas fa-location-crosshairs', mobileLabel: 'GPS' },
];

export default function Tabs({ activeTab, setActiveTab }: TabsProps) {
  return (
    <>
      <nav className="tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={activeTab === tab.id ? 'active' : ''}
            onClick={() => setActiveTab(tab.id)}
          >
            <i className={tab.icon}></i> {tab.label}
          </button>
        ))}
      </nav>

      <nav className="mobile-tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={activeTab === tab.id ? 'active' : ''}
            onClick={() => setActiveTab(tab.id)}
          >
            <strong>
              <i className={tab.icon}></i>
            </strong>
            {tab.mobileLabel}
          </button>
        ))}
      </nav>
    </>
  );
}