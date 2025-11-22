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
    <div className="bg-[#161b22] rounded-lg shadow-lg p-3 md:p-4 xl:p-5 3xl:p-8 4k:p-12 border border-[#30363d] h-full min-h-[200px] lg:min-h-0">
      <h3 className="text-xl md:text-2xl xl:text-3xl 3xl:text-5xl 4k:text-7xl font-bold text-slate-200 mb-2 md:mb-3 xl:mb-4 3xl:mb-6 4k:mb-8">
        Helligdage næste 7 dage
      </h3>
      {isLoading ? (
        <div className="flex items-center justify-center h-[calc(100%-3rem)] md:h-[calc(100%-3.5rem)] xl:h-[calc(100%-4rem)] 3xl:h-[calc(100%-6rem)] 4k:h-[calc(100%-8rem)]">
          <p className="text-slate-400 text-base md:text-lg xl:text-2xl 3xl:text-4xl 4k:text-6xl">Indlæser helligdage...</p>
        </div>
      ) : error ? (
        <div className="flex items-center justify-center h-[calc(100%-3rem)] md:h-[calc(100%-3.5rem)] xl:h-[calc(100%-4rem)] 3xl:h-[calc(100%-6rem)] 4k:h-[calc(100%-8rem)]">
          <p className="text-red-400 text-base md:text-lg xl:text-2xl 3xl:text-4xl 4k:text-6xl">{error}</p>
        </div>
      ) : datesWithHolidays.length > 0 ? (
        <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 lg:grid-cols-7 gap-2 md:gap-3 xl:gap-4 3xl:gap-6 4k:gap-8 h-[calc(100%-3rem)] md:h-[calc(100%-3.5rem)] xl:h-[calc(100%-4rem)] 3xl:h-[calc(100%-6rem)] 4k:h-[calc(100%-8rem)]">
          {datesWithHolidays.slice(0, 7).map(([dateStr, holidaysOnDate]) => (
            <div key={dateStr} className="bg-[#0d1117] rounded-lg p-2 md:p-3 xl:p-4 3xl:p-6 4k:p-8 border border-[#30363d] flex flex-col">
              <h4 className="text-base md:text-xl xl:text-2xl 3xl:text-4xl 4k:text-6xl font-bold text-[#58a6ff] mb-2 md:mb-3 xl:mb-4 3xl:mb-6 4k:mb-8 pb-1.5 md:pb-2 xl:pb-3 3xl:pb-4 4k:pb-6 border-b border-[#30363d] text-center">
                {formatDateForDisplay(dateStr)}
              </h4>
              <div className="flex-1 overflow-y-auto space-y-1.5 md:space-y-2 xl:space-y-3 3xl:space-y-4 4k:space-y-6">
                {holidaysOnDate.map((holiday, index) => (
                  <div key={`${holiday.countryCode}-${holiday.name}-${index}`} className="bg-[#161b22] rounded p-2 md:p-3 xl:p-4 3xl:p-6 4k:p-10 border border-[#30363d]">
                    <div className="font-bold text-slate-100 text-sm md:text-lg xl:text-xl 3xl:text-3xl 4k:text-[5rem] mb-1 md:mb-2 xl:mb-3 3xl:mb-4 4k:mb-6">{holiday.countryName}</div>
                    <div className="text-slate-400 text-xs md:text-base xl:text-lg 3xl:text-2xl 4k:text-[3.5rem] leading-snug" title={holiday.name}>{holiday.name}</div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="flex items-center justify-center h-[calc(100%-3rem)] md:h-[calc(100%-3.5rem)] xl:h-[calc(100%-4rem)] 3xl:h-[calc(100%-6rem)] 4k:h-[calc(100%-8rem)]">
          <p className="text-slate-400 text-base md:text-lg xl:text-2xl 3xl:text-4xl 4k:text-6xl">Ingen helligdage de næste 7 dage.</p>
        </div>
      )}
    </div>
  );
};

export default Holidays;
