import axios, { AxiosError, AxiosResponse } from 'axios';
import type { Holiday } from './interface/holiday';

const api = axios.create({
  baseURL: '/api',
});

const MAX_RETRIES = 10;
const RETRY_DELAY = 2000;

const sleep = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

api.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError) => {
    const config = error.config as any;
    
    // If it's a network error and we haven't exceeded retries, retry
    if ((!error.response || error.response.status >= 500) && (!config._retryCount || config._retryCount < MAX_RETRIES)) {
      config._retryCount = (config._retryCount || 0) + 1;
      console.log(`Backend not ready, retrying (${config._retryCount}/${MAX_RETRIES})...`);
      await sleep(RETRY_DELAY);
      return api(config);
    }
    
    return Promise.reject(error);
  }
);

export const getHolidaysForToday = async (): Promise<Holiday[]> => {
  const response = await api.get('/holidays/today');
  return response.data;
};

export const getUpcomingHolidays = async (): Promise<Holiday[]> => {
  const response = await api.get('/holidays/upcoming');
  return response.data;
};

export default api;