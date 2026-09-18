import { useEffect, useState } from "react";
import "./App.css";

interface Device {
  id: number;
  name: string;
  voltage: number;
  current: number;
  temperature: number;
  status: string;
  alarmAcknowledged: boolean;
}

function App() {
  const [devices, setDevices] = useState<Device[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showAddForm, setShowAddForm] = useState(false);

const [newDevice, setNewDevice] = useState({
  name: "",
  voltage: "",
  current: "",
  temperature: "",
  status: "NORMAL",
});

  const loadDevices = async () => {
    try {
      const response = await fetch("http://localhost:5122/api/devices");

      if (!response.ok) {
        throw new Error("Unable to retrieve device data.");
      }

      const data: Device[] = await response.json();
      setDevices(data);
      setError("");
    } catch {
      setError("Unable to connect to the PowerGrid API.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDevices();
  }, []);

  const acknowledgeAlarm = async (id: number) => {
    try {
      const response = await fetch(
        `http://localhost:5122/api/devices/${id}/acknowledge`,
        { method: "POST" }
      );

      if (!response.ok) {
        throw new Error("Unable to acknowledge alarm.");
      }

      await loadDevices();
    } catch {
      setError("Alarm acknowledgement failed.");
    }
  };

  const addDevice = async (event: React.FormEvent) => {
  event.preventDefault();

  try {
    const response = await fetch("http://localhost:5122/api/devices", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        name: newDevice.name,
        voltage: Number(newDevice.voltage),
        current: Number(newDevice.current),
        temperature: Number(newDevice.temperature),
        status: newDevice.status,
      }),
    });

    if (!response.ok) {
      throw new Error("Unable to add device.");
    }

    setNewDevice({
      name: "",
      voltage: "",
      current: "",
      temperature: "",
      status: "NORMAL",
    });

    setShowAddForm(false);
    await loadDevices();
  } catch {
    setError("Unable to add the new relay.");
  }
};

  const alarmCount = devices.filter(
    (device) => device.status === "ALARM" && !device.alarmAcknowledged
  ).length;

  return (
    <div className="app">
      <header>
        <div>
          <p className="eyebrow">POWER SYSTEM OPERATIONS</p>
          <h1>PowerGrid Device Monitor</h1>
          <p className="subtitle">
            Real-time monitoring dashboard for simulated protection devices
          </p>
        </div>

        <div className={alarmCount > 0 ? "system alarm" : "system healthy"}>
          <span className="status-dot"></span>
          {alarmCount > 0 ? `${alarmCount} ACTIVE ALARM` : "SYSTEM HEALTHY"}
        </div>
      </header>

      <main>
        <section className="summary">
          <div className="summary-card">
            <span>MONITORED DEVICES</span>
            <strong>{devices.length}</strong>
          </div>

          <div className="summary-card">
            <span>ACTIVE ALARMS</span>
            <strong>{alarmCount}</strong>
          </div>

          <div className="summary-card">
            <span>API STATUS</span>
            <strong>{error ? "OFFLINE" : "CONNECTED"}</strong>
          </div>
        </section>

        <section className="panel">
          <div className="panel-heading">
            <div>
              <h2>Protection Devices</h2>
              <p>Electrical measurements and operating status</p>
            </div>

            <div className="panel-actions">
  <button
    className="add-button"
    onClick={() => setShowAddForm(true)}
  >
    + Add Relay
  </button>

  <button className="refresh" onClick={loadDevices}>
    Refresh Data
  </button>
</div>
          </div>
          {showAddForm && (
  <div className="add-form-container">
    <form className="add-form" onSubmit={addDevice}>
      <div className="form-header">
        <div>
          <h3>Add Protection Relay</h3>
          <p>Register a simulated device for monitoring.</p>
        </div>

        <button
          type="button"
          className="close-button"
          onClick={() => setShowAddForm(false)}
        >
          ×
        </button>
      </div>

      <div className="form-grid">
        <label>
          Device Name
          <input
            required
            placeholder="Relay-104"
            value={newDevice.name}
            onChange={(e) =>
              setNewDevice({
                ...newDevice,
                name: e.target.value,
              })
            }
          />
        </label>

        <label>
          Voltage (V)
          <input
            required
            type="number"
            step="0.1"
            min="0"
            placeholder="121.5"
            value={newDevice.voltage}
            onChange={(e) =>
              setNewDevice({
                ...newDevice,
                voltage: e.target.value,
              })
            }
          />
        </label>

        <label>
          Current (A)
          <input
            required
            type="number"
            step="0.1"
            min="0"
            placeholder="15.2"
            value={newDevice.current}
            onChange={(e) =>
              setNewDevice({
                ...newDevice,
                current: e.target.value,
              })
            }
          />
        </label>

        <label>
          Temperature (°C)
          <input
            required
            type="number"
            step="0.1"
            min="0"
            placeholder="43.0"
            value={newDevice.temperature}
            onChange={(e) =>
              setNewDevice({
                ...newDevice,
                temperature: e.target.value,
              })
            }
          />
        </label>

        <label>
          Status
          <select
            value={newDevice.status}
            onChange={(e) =>
              setNewDevice({
                ...newDevice,
                status: e.target.value,
              })
            }
          >
            <option value="NORMAL">NORMAL</option>
            <option value="ALARM">ALARM</option>
          </select>
        </label>
      </div>

      <div className="form-actions">
        <button
          type="button"
          className="cancel-button"
          onClick={() => setShowAddForm(false)}
        >
          Cancel
        </button>

        <button type="submit" className="submit-button">
          Add Relay
        </button>
      </div>
    </form>
  </div>
)}

          {loading && <p>Loading device telemetry...</p>}

          {error && <div className="error">{error}</div>}

          {!loading && !error && (
            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>DEVICE</th>
                    <th>VOLTAGE</th>
                    <th>CURRENT</th>
                    <th>TEMPERATURE</th>
                    <th>STATUS</th>
                    <th>ACTION</th>
                  </tr>
                </thead>

                <tbody>
                  {devices.map((device) => (
                    <tr key={device.id}>
                      <td>
                        <div className="device-name">{device.name}</div>
                        <small>ID {device.id.toString().padStart(3, "0")}</small>
                      </td>

                      <td>{device.voltage.toFixed(1)} V</td>
                      <td>{device.current.toFixed(1)} A</td>

                      <td
                        className={
                          device.temperature >= 70 ? "high-value" : ""
                        }
                      >
                        {device.temperature.toFixed(1)} °C
                      </td>

                      <td>
                        <span
                          className={`badge ${
                            device.status === "ALARM" ? "badge-alarm" : "badge-normal"
                          }`}
                        >
                          {device.status}
                        </span>
                      </td>

                      <td>
                        {device.status === "ALARM" ? (
                          device.alarmAcknowledged ? (
                            <span className="acknowledged">✓ Acknowledged</span>
                          ) : (
                            <button
                              className="ack-button"
                              onClick={() => acknowledgeAlarm(device.id)}
                            >
                              Acknowledge
                            </button>
                          )
                        ) : (
                          <span className="no-action">—</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </main>

      <footer>
        Simulated telemetry • ASP.NET Core API • React + TypeScript
      </footer>
    </div>
  );
}

export default App;