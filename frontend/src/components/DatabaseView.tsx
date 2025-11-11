import { useState, useEffect, useRef } from 'react'; // Added useRef
import api from '../api';
import Header from './Header';

interface Invoice {
  year: number;
  month: number;
  amount: number;
}

interface GroupedInvoices {
  [year: number]: Invoice[];
}

const monthNames: { [key: number]: string } = {
  1: 'Jan', 2: 'Feb', 3: 'Mar', 4: 'Apr', 5: 'May', 6: 'Jun',
  7: 'Jul', 8: 'Aug', 9: 'Sep', 10: 'Oct', 11: 'Nov', 12: 'Dec'
};

const DatabaseView: React.FC = () => {
  const [groupedInvoices, setGroupedInvoices] = useState<GroupedInvoices>({});
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [sortedYears, setSortedYears] = useState<number[]>([]);
  const [selectedYear, setSelectedYear] = useState<number | null>(null);

  const [time, setTime] = useState<string>('');
  const [date, setDate] = useState<string>('');
  const [weekNumber, setWeekNumber] = useState<string>('');

  const [editingCell, setEditingCell] = useState<{ year: number; month: number } | null>(null);
  const [currentEditValue, setCurrentEditValue] = useState<string>('');
  const editInputRef = useRef<HTMLInputElement>(null); // For focusing the input

  const danishNumberFormat = new Intl.NumberFormat('da-DK', {
    style: 'decimal',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  useEffect(() => {
    const updateHeaderData = () => {
      const today = new Date();
      const h = today.getHours().toString().padStart(2, '0');
      const m = today.getMinutes().toString().padStart(2, '0');
      const s = today.getSeconds().toString().padStart(2, '0');
      setTime(`${h}:${m}:${s}`);
      const day = today.getDate().toString().padStart(2, '0');
      const monthStr = (today.getMonth() + 1).toString().padStart(2, '0');
      const year = today.getFullYear();
      setDate(`${day}.${monthStr}.${year}`);
      const startDate = new Date(today.getFullYear(), 0, 1);
      const days = Math.floor((today.getTime() - startDate.getTime()) / (24 * 60 * 60 * 1000));
      const weekNum = Math.ceil(days / 7);
      setWeekNumber(`Uge: ${weekNum}`);
    };
    updateHeaderData();
    const timer = setInterval(updateHeaderData, 1000);
    return () => clearInterval(timer);
  }, []);

  const fetchInvoices = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await api.get<Invoice[]>('/invoices');
      const data = response.data;
      const grouped = data.reduce((acc, invoice) => {
        const year = invoice.year;
        if (!acc[year]) {
          acc[year] = [];
        }
        acc[year].push(invoice);
        acc[year].sort((a, b) => a.month - b.month);
        return acc;
      }, {} as GroupedInvoices);
      setGroupedInvoices(grouped);
      const years = Object.keys(grouped).map(Number).sort((a, b) => b - a); // Sort years descending
      setSortedYears(years);
      if (years.length > 0) {
        setSelectedYear(years[0]); // Select the most recent year by default
      }
    } catch (err) {
      console.error('Error fetching invoices:', err);
      setError('Failed to load invoice data.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchInvoices();
  }, []);

  // Focus input when editingCell changes
  useEffect(() => {
    if (editingCell && editInputRef.current) {
      editInputRef.current.focus();
      editInputRef.current.select(); // Select all text in input
    }
  }, [editingCell]);

  const saveInvoiceAmount = async (year: number, month: number, newAmountStr: string) => {
    // Replace comma with dot for parseFloat if user enters Danish style decimal
    const sanitizedAmountStr = newAmountStr.replace(',', '.');
    const newAmount = parseFloat(sanitizedAmountStr);
    if (isNaN(newAmount)) {
      alert("Invalid amount. Please enter a valid number.");
      return false; // Indicate failure
    }

    const originalInvoicesForYear = groupedInvoices[year] ? [...groupedInvoices[year]] : [];
    
    setGroupedInvoices(prev => {
      const existingInvoiceIndex = prev[year]?.findIndex(inv => inv.month === month);
      let updatedYearInvoices = [...(prev[year] || [])];
      if (existingInvoiceIndex !== undefined && existingInvoiceIndex > -1) {
        updatedYearInvoices[existingInvoiceIndex] = { ...updatedYearInvoices[existingInvoiceIndex], amount: newAmount };
      } else {
        updatedYearInvoices.push({ year, month, amount: newAmount });
        updatedYearInvoices.sort((a,b) => a.month - b.month);
      }
      return { ...prev, [year]: updatedYearInvoices };
    });

    try {
      await api.put(`/invoices/${year}/${month}`, { amount: newAmount });
      return true; // Indicate success
    } catch (err) {
      console.error('Error updating invoice:', err);
      alert('Failed to update invoice. Reverting change.');
      setGroupedInvoices(prev => ({ ...prev, [year]: originalInvoicesForYear }));
      return false; // Indicate failure
    }
  };

  const handleCellClick = (year: number, month: number, currentAmount: number) => {
    if (editingCell && (editingCell.year !== year || editingCell.month !== month)) {
        // If already editing another cell, prompt to save or cancel first, or auto-save/cancel
        // For simplicity, let's just cancel the previous edit
        setEditingCell(null); 
    }
    setEditingCell({ year, month });
    // Store value for input field with dot as decimal separator
    setCurrentEditValue(currentAmount.toFixed(2).replace(',', '.'));
  };

  const handleSaveEdit = async () => {
    if (editingCell) {
      const success = await saveInvoiceAmount(editingCell.year, editingCell.month, currentEditValue);
      if (success) {
        setEditingCell(null);
      }
      // If not successful, keep editing mode active for correction or cancellation
    }
  };

  const handleCancelEdit = () => {
    setEditingCell(null);
  };
  
  const getMonthsForYear = (year: number | null): Array<{ month: number; amount: number; year: number }> => {
    if (!year) return [];
    const yearData: Array<{ month: number; amount: number; year: number }> = [];
    const invoicesForSelectedYear = groupedInvoices[year] || [];

    for (let m = 1; m <= 12; m++) {
      const existingInvoice = invoicesForSelectedYear.find(inv => inv.month === m);
      if (existingInvoice) {
        yearData.push(existingInvoice);
      } else {
        yearData.push({ year: year, month: m, amount: 0.00 });
      }
    }
    return yearData;
  };

  if (isLoading && !time) return <p className="text-center p-4 text-xl text-slate-300">Loading...</p>;
  
  return (
    <div className="min-h-screen bg-[#0d1117]">
      <Header time={time} date={date} weekNumber={weekNumber} />
      <main className="w-full px-12 md:px-20 py-8">
        <h1 className="text-4xl font-bold text-slate-200 mb-8 text-center sm:text-left">
          Invoice Database
        </h1>

        {isLoading && <p className="text-center text-slate-400 py-5 text-lg">Loading invoice data...</p>}
        {error && <p className="text-center text-red-400 py-5 text-lg">{error}</p>}

        {!isLoading && !error && sortedYears.length === 0 && <p className="text-center text-slate-400 text-lg">No invoice data found.</p>}

        {!isLoading && !error && sortedYears.length > 0 && (
          <div className="mb-8 flex flex-wrap justify-center sm:justify-start items-center gap-3">
            <span className="mr-3 self-center font-semibold text-slate-300 text-lg">Select Year:</span>
            {sortedYears.map(year => (
              <button
                key={year}
                onClick={() => setSelectedYear(year)}
                className={`px-5 py-2.5 rounded-md text-lg font-semibold transition-colors 
                  ${selectedYear === year 
                    ? 'bg-[#114C96] text-white' 
                    : 'bg-[#161b22] text-slate-300 hover:bg-[#30363d] border border-[#30363d]'}`}
              >
                {year}
              </button>
            ))}
          </div>
        )}

        {selectedYear && !isLoading && (
          <div key={selectedYear} className="bg-[#161b22] p-8 rounded-lg shadow-lg border border-[#30363d]">
            <h2 className="text-2xl font-semibold text-slate-200 mb-6 text-center">Year: {selectedYear}</h2>
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-4 xl:grid-cols-6 gap-4">
              {getMonthsForYear(selectedYear).map(({ month, amount }) => {
                const isCurrentlyEditing = editingCell?.year === selectedYear && editingCell?.month === month;
                return (
                  <div key={month} className="bg-[#0d1117] p-4 rounded-md border border-[#30363d]">
                    <label htmlFor={`amount-${selectedYear}-${month}`} className="block text-lg font-semibold text-[#58a6ff] mb-2">
                      {monthNames[month]}
                    </label>
                    {isCurrentlyEditing ? (
                      <input
                        ref={isCurrentlyEditing ? editInputRef : null}
                        id={`amount-${selectedYear}-${month}`}
                        type="number"
                        step="0.01"
                        value={currentEditValue}
                        onChange={(e) => setCurrentEditValue(e.target.value)}
                        className="p-2.5 border-2 border-[#58a6ff] rounded-md w-full focus:ring-2 focus:ring-[#58a6ff] focus:border-[#58a6ff] text-white text-lg bg-[#161b22] font-semibold"
                      />
                    ) : (
                      <div
                        onClick={() => handleCellClick(selectedYear, month, amount)}
                        className="p-2.5 border-2 border-transparent rounded-md w-full text-white text-lg cursor-pointer hover:bg-[#161b22] hover:border-[#30363d] min-h-[50px] flex items-center font-semibold transition-colors"
                      >
                        {danishNumberFormat.format(amount)}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
            {editingCell && editingCell.year === selectedYear && (
              <div className="mt-6 flex justify-center gap-3">
                <button
                  onClick={handleSaveEdit}
                  className="px-6 py-2.5 bg-[#114C96] text-white rounded-md hover:bg-[#0d3a75] focus:outline-none focus:ring-2 focus:ring-[#58a6ff] transition-colors text-lg font-semibold"
                >
                  Save Changes
                </button>
                <button
                  onClick={handleCancelEdit}
                  className="px-6 py-2.5 bg-[#30363d] text-white rounded-md hover:bg-[#484f58] focus:outline-none focus:ring-2 focus:ring-[#30363d] transition-colors text-lg font-semibold"
                >
                  Cancel
                </button>
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  );
};

export default DatabaseView;