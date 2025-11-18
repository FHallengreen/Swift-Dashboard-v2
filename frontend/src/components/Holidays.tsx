import { useState, useEffect } from 'react';
import { getUpcomingHolidays } from '../api';
import type { Holiday } from '../interface/holiday';

interface GroupedHolidays {
  [date: string]: Holiday[];
}

const Holidays: React.FC = () => {
  const [upcomingHolidays, setUpcomingHolidays] = useState<Holiday[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchHolidays = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const holidays = await getUpcomingHolidays();
        setUpcomingHolidays(holidays);
      } catch (error) {
        console.error('Error fetching public holidays:', error);
        setError('Failed to load public holidays.');
      } finally {
        setIsLoading(false);
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

    return () => {
      if (holidayFetchTimeoutId) {
        clearTimeout(holidayFetchTimeoutId);
      }
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
    <div className="bg-[#161b22] rounded-lg shadow-lg p-6 border border-[#30363d] h-full">
      <h3 className="text-3xl font-bold text-slate-200 mb-4">
        Helligdage næste 7 dage
      </h3>
      {isLoading ? (
        <div className="flex items-center justify-center h-[calc(100%-4rem)]">
          <p className="text-slate-400 text-2xl">Indlæser helligdage...</p>
        </div>
      ) : error ? (
        <div className="flex items-center justify-center h-[calc(100%-4rem)]">
          <p className="text-red-400 text-2xl">{error}</p>
        </div>
      ) : datesWithHolidays.length > 0 ? (
        <div className="grid grid-cols-7 gap-3 h-[calc(100%-4rem)]">
          {datesWithHolidays.slice(0, 7).map(([dateStr, holidaysOnDate]) => (
            <div key={dateStr} className="bg-[#0d1117] rounded-lg p-3 border border-[#30363d] flex flex-col">
              <h4 className="text-2xl font-bold text-[#58a6ff] mb-3 pb-2 border-b border-[#30363d] text-center">
                {formatDateForDisplay(dateStr)}
              </h4>
              <div className="flex-1 overflow-y-auto space-y-2">
                {holidaysOnDate.map((holiday, index) => (
                  <div key={`${holiday.countryCode}-${holiday.name}-${index}`} className="bg-[#161b22] rounded p-3 border border-[#30363d]">
                    <div className="font-bold text-slate-100 text-xl mb-2">{holiday.countryName}</div>
                    <div className="text-slate-400 text-base leading-snug" title={holiday.name}>{holiday.name}</div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="flex items-center justify-center h-[calc(100%-4rem)]">
          <p className="text-slate-400 text-2xl">Ingen helligdage de næste 7 dage.</p>
        </div>
      )}
    </div>
  );
};

export default Holidays;
