<h1 align="center">🪄 Magic Button</h1>

<p align="center">
  A Raspberry Pi powered hardware trigger for any API.<br>
  Send requests, show feedback, and automate anything - all from a single button.
</p>

<p align="center">
  <a href="#"><img src="https://img.shields.io/badge/.NET-8.0-blue.svg" alt=".NET 8"></a>
  <a href="#"><img src="https://img.shields.io/badge/Platform-Raspberry%20Pi-red.svg" alt="Platform"></a>
  <a href="#"><img src="https://img.shields.io/badge/License-MIT-green.svg" alt="License"></a>
</p>

---

## ⚡ Overview

**Magic Button** is a compact IoT device built around a **Raspberry Pi Zero 2W**.  
It features one physical button and three status LEDs (**Red**, **Green**, **Amber**).  
When pressed, it performs an HTTP request (e.g., **POST**, **GET**, **PUT**) defined in a local database and provides visual feedback based on the API response.

It’s designed for developers, sysadmins, and automation geeks who want a physical way to trigger webhooks, deploys, or scripts - with **no cloud dependency**.

---

## 🔌 Hardware Setup

### 🧱 Components
- Raspberry Pi Zero 2W  
- Momentary push button (tactile or large arcade-style)  
- 3× LEDs (Red, Green, Amber)  
- 3× 330 Ω resistors  
- Breadboard or PCB  
- Jumper wires  

---

### 🪜 Wiring Overview

| Component | GPIO Pin | Physical Pin | Notes |
|------------|-----------|---------------|--------|
| **Red LED** | GPIO 17 | Pin 11 | Error indicator |
| **Green LED** | GPIO 27 | Pin 13 | Success indicator |
| **Amber LED** | GPIO 22 | Pin 15 | Warning indicator |
| **Button** | GPIO 23 | Pin 16 | Trigger input |
| **GND (common)** | — | Pin 6 | Shared ground for all components |

💡 *Each LED’s anode (long leg) connects to the GPIO pin through a 330 Ω resistor. The cathode (short leg) connects to GND.*

🪩 *You can adjust the pin mapping in your app’s configuration (stored in SQLite via Razor UI).*

---

### 📸 Example Breadboard Layout
[ GPIO 17 ] --▸[330Ω]--▸(+) Red LED (-)▸ GND

[ GPIO 27 ] --▸[330Ω]--▸(+) Green LED (-)▸ GND

[ GPIO 22 ] --▸[330Ω]--▸(+) Amber LED (-)▸ GND

[ GPIO 23 ] --▸ Push Button ▸ GND


---

## 🧠 Core Features

### ✅ Single / Double / Long Press Actions
Each press type can perform a different function — single press to call your webhook, double to restart the Pi, or long press for a controlled shutdown.

### 💡 LED Response Mapping
Each LED can represent specific HTTP response codes:

| LED | Meaning |
|-----|----------|
| 🟢 Green | 200 OK |
| 🟠 Amber | 202 Accepted / 400-level warnings |
| 🔴 Red | 500+ errors |

### ⚙️ Fully Configurable via Razor UI
Magic Button runs a lightweight **Razor Pages web app** served locally on the Pi.  
Users can define:
- API endpoint and method  
- Request body or query string  
- Response-to-LED mapping  
- Press behaviour (single / double / long)

### 🪶 Local Footprint
Built with **.NET 8**, **System.Device.Gpio**, and **SQLite**.  
Self-contained deployment - no external dependencies.

### 🕒 Background Task Handling
Integrates with **IHostedService** for scheduled retries or asynchronous background jobs.

### 🔍 Local Logging
Uses **Serilog** for structured logging, viewable realtime via a Razor page in the browser UI.

---

## 🧩 Tech Stack

- Raspberry Pi
- System.Device.Gpio    
- .NET 8 Razor Pages WebApp  
- Entity Framework (SQLite)
- HTMX

---

## 🧰 Example Use Cases

- 🚀 Trigger a CI/CD deployment  
- 🧩 Call a webhook (e.g., GitHub Actions, Teams message, IFTTT event)  
- 💻 Restart a web service or clear a cache  
- 🌡️ Report system status or send a notification  
- 💡 Integrate with smart-home APIs  
- 📈 Post a metric or log event to an API  

---

## 🧠 Future Ideas

- 🌐 MQTT / Home Assistant support  
- 🔄 Over-the-air config sync    
- 🧱 Multi-button support  
- 🔔 Buzzer or OLED display feedback  
- 🧠 AI integration (e.g.,mode that transcribes voice input → API calls)

---

## 🧵 Example Flow

1. Press the Magic Button  
2. The Pi executes a POST request to your configured API  
3. The HTTP response is logged and parsed  
4. The corresponding LED lights up (e.g., 🟢 Green for success)  
5. After a few seconds, all LEDs reset to idle  
