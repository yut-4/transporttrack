import { defineConfig, devices } from '@playwright/test';

const PORT = 5173;
const API_PORT = 5000;

export default defineConfig({
  testDir: './e2e',
  timeout: 90_000,
  fullyParallel: false,
  workers: 1,
  retries: 1,
  reporter: [['list']],
  use: {
    baseURL: `http://localhost:${PORT}`,
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      command: `dotnet run --project ../TransportTrack.Api/TransportTrack.Api.csproj --urls http://localhost:${API_PORT} --no-build`,
      url: `http://localhost:${API_PORT}/api/health`,
      reuseExistingServer: true,
      timeout: 60_000,
    },
    {
      command: 'npm run dev',
      url: `http://localhost:${PORT}`,
      reuseExistingServer: true,
      timeout: 60_000,
      env: {
        VITE_SIGNALR_URL: `http://localhost:${API_PORT}`,
      },
    },
  ],
});