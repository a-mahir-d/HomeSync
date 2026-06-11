import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LanguageService } from '../../services/language.service';
import { CommonModule } from '@angular/common';
import { Subject, Subscription, switchMap, takeUntil, timer } from 'rxjs';
import { SensorDataService } from '../../services/sensor.data.service';
import { Item, SensorReadEvent } from '../../models/sensor.data.models';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit, OnDestroy {
  protected readonly langService = inject(LanguageService);
  protected readonly sensorDataService = inject(SensorDataService);

  isSimulatorRunning = signal<boolean>(false);
  isLoading = signal<boolean>(false);

  hardwareError = signal<string | null>(null);

  private readonly destroy$ = new Subject<void>();

  items = signal<Item[]>([]);

  ngOnInit(): void {
    this.getItems();
    this.getInitialStatus();
    this.sensorDataService.startSignalRConnection();
    this.listenToSignalREvents();
  }

  ngOnDestroy(): void {
    this.sensorDataService.stopSignalRConnection();
    this.destroy$.next();
    this.destroy$.complete();
  }

  private getInitialStatus(): void {
    this.sensorDataService.getSimulatorStatus()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => this.isSimulatorRunning.set(res),
        error: (err) => console.error('Failed to get simulator status', err)
      });
  }

  private getItems(): void {
    this.sensorDataService.getItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          // İlk açılışta currentDegree = minDegree ataması yapıyoruz
          const mappedItems = res.map(item => ({
            ...item,
            currentDegree: item.minDegree,
            isAlarm: false
          }));
          this.items.set(mappedItems);
        },
        error: (err) => console.error('Failed to get items', err)
      });
  }

  private listenToSignalREvents(): void {
    this.sensorDataService.sensorData$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event: SensorReadEvent) => {
        this.updateItemInList(event);
      });

    // 2. Alarm Akışı Dinleyicisi
    this.sensorDataService.sensorAlarm$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event: SensorReadEvent) => {
        this.updateItemInList(event);
      });
  }

  private updateItemInList(event: SensorReadEvent): void {
    this.items.update(currentItems => 
      currentItems.map(item => {
        if (item.id === event.id) {
          return {
            ...item,
            currentDegree: event.value,
            isAlarm: event.isAlarm
          };
        }
        return item;
      })
    );
  }

  toggleSimulator(): void {
    this.isLoading.set(true);
    const action = this.isSimulatorRunning() 
      ? this.sensorDataService.startSimulator() 
      : this.sensorDataService.startSimulator();

    action.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isSimulatorRunning.update(v => !v);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.isLoading.set(false);
      }
    });
  }
}
