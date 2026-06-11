import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { Observable, Subject } from "rxjs";
import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { AuthService } from "./auth.service";
import { Item, SensorReadEvent } from "../models/sensor.data.models";

@Injectable({
  providedIn: 'root'
})
export class SensorDataService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = `${environment.serverUrl}/api/simulator`;
  private readonly hubUrl = `${environment.serverUrl}/sensorHub`;

  private hubConnection?: HubConnection;

  private readonly sensorDataSource = new Subject<SensorReadEvent>();
  sensorData$ = this.sensorDataSource.asObservable();

  private readonly sensorAlarmSource = new Subject<SensorReadEvent>();
  sensorAlarm$ = this.sensorAlarmSource.asObservable();

  startSignalRConnection(): void {
    if (this.hubConnection?.state === HubConnectionState.Connected) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.authService.getToken() || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('[SignalR] Connected to SensorHub successfully using AuthService token.'))
      .catch(err => console.error('[SignalR] Error while starting connection:', err));

    this.hubConnection.on('ReceiveSensorData', (event: SensorReadEvent) => {
      this.sensorDataSource.next(event);
    });

    this.hubConnection.on('ReceiveSensorAlarm', (event: SensorReadEvent) => {
      this.sensorAlarmSource.next(event);
    });
  }

  stopSignalRConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('[SignalR] Connection stopped.'))
        .catch(err => console.error('[SignalR] Error while stopping connection:', err));
    }
  }

  getItems(): Observable<Item[]> {
    return this.http.get<Item[]>(`${this.baseUrl}/items`);
  }

  getSimulatorStatus(): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/status`);
  }

  startSimulator(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/start`, {});
  }

  stopSimulator(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/stop`, {});
  }
}