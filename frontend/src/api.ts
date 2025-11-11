import axios from 'axios';
import type { Holiday } from './interface/holiday';

const api = axios.create({
  baseURL: '/api',
});

export const getHolidaysForToday = async (): Promise<Holiday[]> => {
  const response = await api.get('/holidays/today');
  return response.data;
};

export const getUpcomingHolidays = async (): Promise<Holiday[]> => {
  const response = await api.get('/holidays/upcoming');
  return response.data;
};

export default api;