import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { defineCustomElements } from '@ionic/core/loader';
import '@ionic/react/css/core.css';
import './index.css';
import App from './App.tsx';
import DriverTrackingPage from './features/tracking/DriverTrackingPage.tsx';
import { ThemeProvider } from './theme/ThemeProvider';

defineCustomElements(window);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/tracking" element={<DriverTrackingPage />} />
          <Route path="*" element={<App />} />
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  </StrictMode>,
);