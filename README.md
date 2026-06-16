# [HomeSync](https://homesync-2t4e.onrender.com)

* MassTransit, RabbitMQ ve SignalR tabanlı, anlık alarm akışlı ve akıllı hata maskelemeli dağıtık IoT sensör izleme paneli.
* A distributed IoT sensor monitoring dashboard built with MassTransit, RabbitMQ, and SignalR, featuring real-time alarm streams and smart client-side error masking.

---

## 🛠️ Tech Stack / Teknolojiler

### 🇹🇷 Türkçe
### Backend (.NET 10)
* **MassTransit & RabbitMQ (CloudAMQP):** Dağıtık mimaride güvenilir, asenkron ve gevşek bağlı (loosely coupled) mesaj kuyruğu yönetimi.
* **SignalR Hub:** JWT token doğrulamalı, arka plan tüketici katmanından (Consumer) ön yüze anlık veri ve arıza akışı sağlayan WebSocket altyapısı.
* **Hosted Background Services:** Kuyruğu besleyen bağımsız sensör veri simülatörü ve hata enjeksiyon motoru.
* **JWT Authentication:** Güvenli, token tabanlı kimlik doğrulama katmanı.

### Frontend (Angular 21)
* **Angular Signals:** Canlı sensör kartlarındaki anlık derece ve alarm değişimleri için optimize edilmiş hafif, reaktif durum yönetimi.
* **Tailwind CSS v4:** Koyu/açık tema uyumlu, modern ve minimal endüstriyel panel tasarımı.
* **RxJS Streams:** Güvenli abonelik yönetimi (`takeUntil`, `Subject`) ile asenkron SignalR event dinleyicileri.

### English
### Backend (.NET 10)
* **MassTransit & RabbitMQ (CloudAMQP):** Reliable, asynchronous, and loosely coupled message broker management for distributed event-driven architecture.
* **SignalR Hub:** Token-authenticated WebSocket infrastructure broadcasting live telemetry data and critical hardware faults from the Consumer layer to clients.
* **Hosted Background Services:** Independent background telemetry generator with an integrated fault injection simulation engine.
* **JWT Authentication:** Secure token-based authentication layer.

### Frontend (Angular 21)
* **Angular Signals:** Lightweight, highly optimized reactive state management tailored for real-time sensor updates and state mutations.
* **Tailwind CSS v4:** Minimalist industrial UI design with native out-of-the-box dark/light mode support.
* **RxJS Streams:** Memory-safe subscription handling (`takeUntil`, `Subject`) for continuous SignalR hub events.

---

## ⚙️ Core Logic / Temel Mantık

### 🇹🇷 Türkçe
* **Dağıtık Veri Hattı:** Background Worker sensör verisi üretir $\rightarrow$ Veri MassTransit aracılığıyla CloudAMQP üzerindeki RabbitMQ kuyruğuna basılır $\rightarrow$ `SensorDataConsumer` mesajı asenkron olarak tüketir $\rightarrow$ Verinin durumuna göre ilgili SignalR kanalından ön yüze anlık fırlatılır.
* **Hata Maskeleme (Fail-Safe) & İstemci Tabanlı Dil Yönetimi:** Arka planda %10 olasılıkla oluşan donanım arızalarında backend sadece ham bir kod (`SENSOR_ERROR_ON_X`) fırlatır. Ön yüz bu kodu yakalayarak kullanıcının tarayıcı diline (TR/ENG) göre dinamik bir uyarı kutusuna dönüştürür ve sistem kararlılığı için arızalı sensörün derecesini geçici olarak `0°C` şeklinde maskeler.
* **Kaynak Optimizasyonu (Auto-Stop):** Render, Neon ve CloudAMQP (Free Tier) kotalarını korumak amacıyla, SignalR Hub'ında aktif hiçbir istemci kalmadığı an arka plan simülatör döngüsü thread güvenli (`Lock`) olarak otomatik askıya alınır.

### English
* **Distributed Data Pipeline:** Background Worker generates telemetry data $\rightarrow$ Publishes it to RabbitMQ on CloudAMQP via MassTransit $\rightarrow$ `SensorDataConsumer` consumes the message asynchronously $\rightarrow$ Broadcasts payload to clients using conditional SignalR events based on item states.
* **Fail-Safe Masking & Client-Side Localization:** When a hardware fault occurs (10% probability), the backend pushes a raw identifier (`SENSOR_ERROR_ON_X`). The frontend captures this code, localizes the error message based on active language settings (TR/ENG), and temporarily masks the damaged sensor's degree as `0°C` to provide a clean UX.
* **Resource Optimization (Auto-Stop):** To prevent exceeding Render, Neon, and CloudAMQP free-tier limits, the background simulator tracking loop is safely suspended using thread synchronization (`Lock`) as soon as the last client disconnects from the SignalR Hub.
