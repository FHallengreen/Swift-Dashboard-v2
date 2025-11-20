import { useState, useEffect } from 'react';
import api from '../api';
import * as signalR from '@microsoft/signalr';

interface Invoice {
  year: number;
  month: number;
  amount: number;
}

const danishWholeNumberFormat = new Intl.NumberFormat('da-DK', {
  style: 'decimal',
  minimumFractionDigits: 0,
  maximumFractionDigits: 0,
});

const danishDecimalDisplayFormat = new Intl.NumberFormat('da-DK', {
  style: 'decimal',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const DagensTal: React.FC = () => {
  const [currentInvoice, setCurrentInvoice] = useState<Invoice>({
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    amount: 0,
  });
  const [amountInput, setAmountInput] = useState<string>('');
  const [displayAmountInput, setDisplayAmountInput] = useState<string>('');

  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCurrentInvoice = () => {
      setIsLoading(true);
      api
        .get('/invoices/current')
        .then((res) => {
          setCurrentInvoice(res.data);
          setAmountInput(res.data.amount.toString());
          setDisplayAmountInput(danishWholeNumberFormat.format(res.data.amount));
        })
        .catch((error) => {
          console.error('Error fetching current invoice:', error);
          setError('Failed to load Dagens Tal');
        })
        .finally(() => setIsLoading(false));
    };

    fetchCurrentInvoice();

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/api/invoiceHub')
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveInvoiceUpdate", (data: Invoice) => {
      console.log("SignalR: ReceiveInvoiceUpdate", data);
      const today = new Date();
      if (data.year === today.getFullYear() && data.month === (today.getMonth() + 1)) {
        setCurrentInvoice(data);
        setAmountInput(data.amount.toString());
        if (!isEditing) {
            setDisplayAmountInput(danishWholeNumberFormat.format(data.amount));
        }
      }
      localStorage.setItem('invoiceDataTimestamp', Date.now().toString());
      window.dispatchEvent(new CustomEvent('invoicedataupdated'));
    });

    connection.start()
      .then(() => console.log('SignalR Connected for DagensTal'))
      .catch(err => console.error('SignalR Connection Error: ', err));

    return () => {
      connection.stop();
    };
  }, [isEditing]);

  const handleAmountInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const inputValue = e.target.value;
    const rawNumericValue = inputValue.replace(/\D/g, '');

    setAmountInput(rawNumericValue);

    if (rawNumericValue === '') {
      setDisplayAmountInput('');
    } else {
      const num = parseInt(rawNumericValue, 10);
      if (!isNaN(num)) {
        setDisplayAmountInput(danishWholeNumberFormat.format(num));
      } else {
        setDisplayAmountInput(rawNumericValue); 
      }
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (amountInput === '') {
        setError('Please enter a valid number');
        return;
    }
    const newAmount = parseInt(amountInput, 10);
    if (isNaN(newAmount)) {
      setError('Please enter a valid number');
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      await api.post('/invoices', newAmount, {
        headers: { 'Content-Type': 'application/json' },
      });
      setCurrentInvoice({ ...currentInvoice, amount: newAmount });
      setDisplayAmountInput(danishWholeNumberFormat.format(newAmount));
      setIsEditing(false);
    } catch (error) {
      console.error('Error submitting invoice:', error);
      setError('Failed to update Dagens Tal');
    } finally {
      setIsLoading(false);
    }
  };

  const formattedAmount = `${danishDecimalDisplayFormat.format(currentInvoice.amount)} EUR`;

  return (
    <div className="h-full flex flex-col">
      <h2 className="text-2xl md:text-3xl xl:text-4xl font-semibold text-slate-200 mb-3 md:mb-4 xl:mb-6">Dagens Tal</h2>
      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-slate-400 text-lg md:text-xl xl:text-2xl">Loading...</p>
        </div>
      ) : error ? (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-red-400 text-lg md:text-xl xl:text-2xl">{error}</p>
        </div>
      ) : (
        <div className="flex-1 flex flex-col items-center justify-center gap-3 md:gap-4 xl:gap-6">
          {isEditing ? (
            <form onSubmit={handleSubmit} className="w-full flex flex-col gap-3 md:gap-3.5 xl:gap-4">
              <input
                type="text"
                value={displayAmountInput}
                onChange={handleAmountInputChange}
                className="text-3xl md:text-4xl xl:text-5xl font-bold text-center border-2 border-[#58a6ff] rounded-md p-2 md:p-3 xl:p-4 bg-[#0d1117] text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-[#58a6ff] focus:border-[#58a6ff]"
                placeholder="0"
                autoFocus
              />
              <div className="flex gap-2 md:gap-2.5 xl:gap-3">
                <button
                  type="submit"
                  disabled={isLoading}
                  className="flex-1 px-3 py-2 md:px-4 md:py-2.5 xl:px-6 xl:py-3 text-base md:text-lg xl:text-xl font-semibold bg-[#114C96] text-white rounded-md hover:bg-[#0d3a75] transition-colors disabled:opacity-50"
                >
                  Save
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setIsEditing(false);
                    setAmountInput(currentInvoice.amount.toString());
                    setDisplayAmountInput(danishWholeNumberFormat.format(currentInvoice.amount));
                    setError(null);
                  }}
                  disabled={isLoading}
                  className="flex-1 px-3 py-2 md:px-4 md:py-2.5 xl:px-6 xl:py-3 text-base md:text-lg xl:text-xl font-semibold bg-[#30363d] text-white rounded-md hover:bg-[#484f58] transition-colors disabled:opacity-50"
                >
                  Cancel
                </button>
              </div>
              {error && <p className="text-red-400 text-center text-sm md:text-base xl:text-lg">{error}</p>}
            </form>
          ) : (
            <button
              onClick={() => setIsEditing(true)}
              className="text-3xl md:text-4xl lg:text-5xl xl:text-6xl font-bold text-white hover:text-[#58a6ff] transition-colors cursor-pointer"
            >
              {formattedAmount}
            </button>
          )}
        </div>
      )}
    </div>
  );
};

export default DagensTal;
