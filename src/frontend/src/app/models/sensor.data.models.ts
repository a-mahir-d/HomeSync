export interface SensorReadEvent {
  id: number;
  value: number;
  isAlarm: boolean;
  timestamp: string;
}

export interface Item {
  id: number;
  name: string;
  minDegree: number;
  maxDegree: number;
  currentDegree?: number;
  isAlarm?: boolean;
}