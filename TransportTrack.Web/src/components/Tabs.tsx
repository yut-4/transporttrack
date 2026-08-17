interface TabsProps {
  activeTab: string;
  setActiveTab: (tab: string) => void;
}

interface TabItem {
  id: string;
  label: string;
  icon: string;
}

const tabs: TabItem[] = [
  { id: 'dashboard', label: 'Dashboard', icon: 'fas fa-gauge-high' },
  { id: 'conductores', label: 'Conductores', icon: 'fas fa-user-tie' },
  { id: 'vehiculos', label: 'Vehículos', icon: 'fas fa-truck' },
  { id: 'rutas', label: 'Rutas', icon: 'fas fa-route' },
];

export default function Tabs({ activeTab, setActiveTab }: TabsProps) {
  return (
    <div className="tabs">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          className={activeTab === tab.id ? 'active' : ''}
          onClick={() => setActiveTab(tab.id)}
        >
          <i className={tab.icon}></i> {tab.label}
        </button>
      ))}
    </div>
  );
}
