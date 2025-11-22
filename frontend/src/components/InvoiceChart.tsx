import { useState, useEffect, useCallback } from 'react';
import { Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  type ChartOptions,
} from 'chart.js';
import ChartDataLabels, { type Context } from 'chartjs-plugin-datalabels'; // Use "type Context"
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ChartDataLabels);

import api from '../api';

interface Invoice {
  year: number;
  month: number;
  amount: number;
}

const InvoiceChart: React.FC = () => {
  const [invoiceData, setInvoiceData] = useState<Invoice[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchInvoiceData = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await api.get('/invoices');
      setInvoiceData(response.data);
    } catch (err) {
      console.error('Error fetching invoice data:', err);
      setError('Failed to load invoice data.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchInvoiceData();

    // Listen for custom event when invoice data is updated by DagensTal or SignalR
    const handleInvoiceDataUpdated = () => {
      console.log('InvoiceChart: invoicedataupdated event received');
      fetchInvoiceData(); // Refetch data when notified
    };

    window.addEventListener('invoicedataupdated', handleInvoiceDataUpdated);

    return () => {
      window.removeEventListener('invoicedataupdated', handleInvoiceDataUpdated);
    };
  }, [fetchInvoiceData]);

  const xValues = ["Januar", "Februar", "Marts", "April", "Maj", "Juni", "Juli", "August", "September", "Oktober", "November", "December"];
  const chartData = {
    labels: xValues,
    datasets: [
      {
        label: '2024',
        backgroundColor: 'rgba(100, 116, 139, 0.7)',
        borderColor: 'rgba(100, 116, 139, 1)',
        borderWidth: 0,
        borderRadius: 4,
        data: xValues.map((_, i) => {
          const inv = invoiceData.find((inv) => inv.year === 2024 && inv.month === i + 1);
          return inv ? inv.amount : 0;
        }),
      },
      {
        label: '2025',
        backgroundColor: 'rgba(17, 76, 150, 0.9)',
        borderColor: 'rgba(17, 76, 150, 1)',
        borderWidth: 0,
        borderRadius: 4,
        data: xValues.map((_, i) => {
          const inv = invoiceData.find((inv) => inv.year === 2025 && inv.month === i + 1);
          return inv ? inv.amount : 0;
        }),
      },
    ],
  };

  const chartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        beginAtZero: true,
        grace: '5%',
        ticks: {
          font: { size: window.innerWidth >= 3840 ? 24 : window.innerWidth >= 1920 ? 18 : window.innerWidth >= 1280 ? 16 : window.innerWidth >= 768 ? 14 : 12 },
          color: '#94a3b8',
          callback: (tickValue: string | number): string => {
            return new Intl.NumberFormat('da-DK', { maximumSignificantDigits: 3 }).format(Number(tickValue));
          },
        },
        grid: {
          color: 'rgba(48, 54, 61, 0.5)',
        },
      },
      x: {
        ticks: {
          font: { size: window.innerWidth >= 3840 ? 28 : window.innerWidth >= 1920 ? 20 : window.innerWidth >= 1280 ? 16 : window.innerWidth >= 768 ? 14 : 12 },
          color: '#94a3b8',
        },
        grid: {
          display: false,
        },
      },
    },
    plugins: {
      legend: { 
        display: true,
        position: 'top',
        labels: {
          color: '#cbd5e1',
          font: {
            size: window.innerWidth >= 3840 ? 32 : window.innerWidth >= 1920 ? 22 : window.innerWidth >= 1280 ? 18 : window.innerWidth >= 768 ? 16 : 14
          },
          padding: window.innerWidth >= 3840 ? 20 : window.innerWidth >= 1920 ? 16 : window.innerWidth >= 1280 ? 12 : window.innerWidth >= 768 ? 10 : 8,
          usePointStyle: true,
          pointStyle: 'circle'
        }
      },
      datalabels: {
        display: (context: Context) => {
          const isActive = context.active;
          const dataPoint = context.dataset.data?.[context.dataIndex];
          const valueIsPositive = typeof dataPoint === 'number' && dataPoint > 0;
          return isActive && valueIsPositive;
        },
        anchor: 'end',
        align: 'end',
        offset: -15,
        color: '#ffffff',
        font: { size: window.innerWidth >= 3840 ? 28 : window.innerWidth >= 1920 ? 20 : window.innerWidth >= 1280 ? 14 : window.innerWidth >= 768 ? 12 : 11, weight: 'bold' },
        textAlign: 'center',
        padding: 3,
      },
      tooltip: {
        enabled: true,
        backgroundColor: 'rgba(22, 27, 34, 0.95)',
        titleColor: '#ffffff',
        bodyColor: '#cbd5e1',
        borderColor: 'rgba(88, 166, 255, 0.5)',
        borderWidth: 1,
        padding: window.innerWidth >= 3840 ? 16 : window.innerWidth >= 1920 ? 12 : window.innerWidth >= 768 ? 10 : 6,
        titleFont: {
          size: window.innerWidth >= 3840 ? 24 : window.innerWidth >= 1920 ? 18 : window.innerWidth >= 1280 ? 14 : window.innerWidth >= 768 ? 12 : 10
        },
        bodyFont: {
          size: window.innerWidth >= 3840 ? 22 : window.innerWidth >= 1920 ? 16 : window.innerWidth >= 1280 ? 13 : window.innerWidth >= 768 ? 11 : 9
        }
      }
    },
  };

  return (
    <div className="h-full flex flex-col">
      <h3 className="text-base md:text-lg xl:text-xl 2xl:text-2xl 4k:text-4xl font-semibold text-slate-200 mb-1 md:mb-2 xl:mb-2 2xl:mb-3 4k:mb-4">Invoice Overview</h3>
      {isLoading ? (
        <p className="text-center text-slate-400 py-4 md:py-6 xl:py-8 2xl:py-10 4k:py-16 text-sm md:text-base xl:text-xl 2xl:text-3xl 4k:text-5xl">Loading chart...</p>
      ) : error ? (
        <p className="text-center text-red-400 py-4 md:py-6 xl:py-8 2xl:py-10 4k:py-16 text-sm md:text-base xl:text-xl 2xl:text-3xl 4k:text-5xl">{error}</p>
      ) : (
        <div className="flex-1 min-h-0">
          <Bar data={chartData} options={chartOptions} />
        </div>
      )}
    </div>
  );
};

export default InvoiceChart;