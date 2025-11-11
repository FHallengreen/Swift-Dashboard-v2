import { useState, useEffect } from 'react';
import Header from './Header';
import Clocks from './Clocks';
import InvoiceChart from './InvoiceChart';
import { getUpcomingHolidays } from '../api';
import type { Holiday } from '../interface/holiday';
import GeneralInfo from './GeneralInfo';
import DagensTal from './DagensTal';

interface GroupedHolidays {
  [date: string]: Holiday[];
}

const Dashboard: React.FC = () => {
  const [time, setTime] = useState<string>('');
  const [date, setDate] = useState<string>('');
  const [weekNumber, setWeekNumber] = useState<string>('');
  const [upcomingHolidays, setUpcomingHolidays] = useState<Holiday[]>([]);
  const [holidaysLoading, setHolidaysLoading] = useState<boolean>(true);
  const [holidaysError, setHolidaysError] = useState<string | null>(null);

  useEffect(() => {
    const updateTime = () => {
      const today = new Date();
      const h = today.getHours().toString().padStart(2, '0');
      const m = today.getMinutes().toString().padStart(2, '0');
      const s = today.getSeconds().toString().padStart(2, '0');
      setTime(`${h}:${m}:${s}`);

      const day = today.getDate().toString().padStart(2, '0');
      const month = (today.getMonth() + 1).toString().padStart(2, '0');
      const year = today.getFullYear();
      setDate(`${day}.${month}.${year}`);

      const startDate = new Date(today.getFullYear(), 0, 1);
      const days = Math.floor((today.getTime() - startDate.getTime()) / (24 * 60 * 60 * 1000));
      const weekNum = Math.ceil(days / 7);
      setWeekNumber(`Uge: ${weekNum}`);
    };
    updateTime();
    const timeUpdateTimer = setInterval(updateTime, 1000);

    const fetchHolidays = async () => {
      setHolidaysLoading(true);
      setHolidaysError(null);
      try {
        const holidays = await getUpcomingHolidays();
        setUpcomingHolidays(holidays);
      } catch (error) {
        console.error('Error fetching public holidays:', error);
        setHolidaysError('Failed to load public holidays.');
      } finally {
        setHolidaysLoading(false);
      }
    };

    let holidayFetchTimeoutId: number | undefined;
    const scheduleNextHolidayFetch = () => {
      if (holidayFetchTimeoutId) clearTimeout(holidayFetchTimeoutId);
      const now = new Date();
      const tomorrow = new Date(now);
      tomorrow.setDate(now.getDate() + 1);
      tomorrow.setHours(0, 0, 0, 0);
      const msUntilMidnight = tomorrow.getTime() - now.getTime();
      holidayFetchTimeoutId = window.setTimeout(async () => {
        await fetchHolidays();
        scheduleNextHolidayFetch();
      }, msUntilMidnight);
    };

    fetchHolidays();
    scheduleNextHolidayFetch();

    // Refresh page at 3 AM
    const refreshAt3AM = () => {
      const now = new Date();
      const target = new Date(now);
      target.setDate(now.getDate() + 1);
      target.setHours(3, 0, 0, 0);
      const timeout = target.getTime() - now.getTime();

      return setTimeout(() => {
        console.log('Refreshing page at 3 AM');
        window.location.reload();
      }, timeout);
    };

    const refreshTimeoutId = refreshAt3AM();

    return () => {
      clearInterval(timeUpdateTimer);
      if (holidayFetchTimeoutId) {
        clearTimeout(holidayFetchTimeoutId);
      }
      clearTimeout(refreshTimeoutId);
    };
  }, []);

  const groupHolidaysByDate = (holidays: Holiday[]): GroupedHolidays => {
    return holidays.reduce((acc, holiday) => {
      const dateKey = holiday.date || 'UnknownDate';
      if (!acc[dateKey]) {
        acc[dateKey] = [];
      }
      acc[dateKey].push(holiday);
      return acc;
    }, {} as GroupedHolidays);
  };

  const formatDateForDisplay = (dateString: string) => {
    if (dateString === 'UnknownDate') return 'Date Unknown';
    const dateObj = new Date(dateString);
    if (isNaN(dateObj.getTime())) {
      return dateString;
    }
    return dateObj.toLocaleDateString('da-DK', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
    });
  };

  const groupedUpcomingHolidays = groupHolidaysByDate(upcomingHolidays);

  const datesWithHolidays = Object.entries(groupedUpcomingHolidays)
    .filter(([, holidaysOnDate]) => holidaysOnDate.length > 0)
    .sort(([dateA], [dateB]) => new Date(dateA).getTime() - new Date(dateB).getTime());

  return (
    <div className="h-screen w-screen flex flex-col bg-[#0d1117] overflow-hidden">
      <Header time={time} date={date} weekNumber={weekNumber} />
      
      <main className="flex-1 w-full px-10 py-6 overflow-y-auto">
        <div className="h-full flex flex-col gap-8">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-2">
            <div className="flex flex-col gap-2 h-full">
              <div className="bg-[#161b22] rounded-lg shadow-lg p-6 border border-[#30363d] flex-1">
                <Clocks />
              </div>
              <div className="bg-[#161b22] rounded-lg shadow-lg p-6 border border-[#30363d] flex-1">
                <DagensTal />
              </div>
            </div>
            <div className="bg-[#161b22] rounded-lg shadow-lg p-6 border border-[#30363d] flex-1">
              <GeneralInfo />
            </div>
          </div>

          {/* Holidays Section - Compact Grid Layout */}
          <div className="bg-[#161b22] rounded-lg shadow-lg p-3 border border-[#30363d]">
            <h3 className="text-2xl font-bold text-slate-200 mb-4">
              Helligdage næste 5 dage
            </h3>
            {holidaysLoading ? (
              <p className="text-slate-400 text-center py-4 text-lg">Indlæser helligdage...</p>
            ) : holidaysError ? (
              <p className="text-red-400 text-center py-4 text-lg">{holidaysError}</p>
            ) : datesWithHolidays.length > 0 ? (
              <div className="grid grid-cols-5 gap-2">
                {datesWithHolidays.slice(0, 5).map(([dateStr, holidaysOnDate]) => (
                  <div key={dateStr} className="bg-[#0d1117] rounded-lg p-2 border border-[#30363d]">
                    <h4 className="text-xl font-bold text-[#58a6ff] mb-4 pb-1 border-b border-[#30363d] text-center">
                      {formatDateForDisplay(dateStr)}
                    </h4>
                    <div className="grid grid-cols-2 gap-2">
                      {holidaysOnDate.map((holiday, index) => (
                        <div key={`${holiday.countryCode}-${holiday.name}-${index}`} className="bg-[#161b22] rounded p-1 border border-[#30363d]">
                          <div className="font-bold text-slate-100 text-sm mb-2 truncate">{holiday.countryName}</div>
                          <div className="text-slate-400 text-xs leading-tight line-clamp-2" title={holiday.name}>{holiday.name}</div>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-slate-400 text-center py-4 text-lg">Ingen helligdage de næste 5 dage.</p>
            )}
          </div>

          {/* Invoice Chart */}
          <div className="bg-[#161b22] rounded-lg shadow-lg p-6 border border-[#30363d]">
            <InvoiceChart />
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;