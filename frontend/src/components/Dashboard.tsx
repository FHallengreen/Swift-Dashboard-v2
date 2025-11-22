import { useState, useEffect } from 'react';
import Header from './Header';
import Clocks from './Clocks';
import InvoiceChart from './InvoiceChart';
import GeneralInfo from './GeneralInfo';
import DagensTal from './DagensTal';
import Holidays from './Holidays';

const Dashboard: React.FC = () => {
  const [time, setTime] = useState<string>('');
  const [date, setDate] = useState<string>('');
  const [weekNumber, setWeekNumber] = useState<string>('');

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
      clearTimeout(refreshTimeoutId);
    };
  }, []);

  return (
    <div className="h-screen w-screen flex flex-col bg-[#0d1117] overflow-hidden">
      <Header time={time} date={date} weekNumber={weekNumber} />
      
      <main className="flex-1 w-full px-2 py-4 md:px-4 md:py-5 xl:px-6 xl:py-6 3xl:px-10 3xl:py-8 4k:px-16 4k:py-12 overflow-y-auto 3xl:overflow-hidden">
        <div className="min-h-full 3xl:h-full flex flex-col gap-4 md:gap-5 xl:gap-6 3xl:gap-8 4k:gap-12">
          {/* Top Row: Left side (Clocks + Dagens Tal) and Right side (General Info) */}
          <div className="grid grid-cols-1 lg:grid-cols-[45fr_55fr] 3xl:grid-cols-[40fr_60fr] gap-2 md:gap-2 xl:gap-6 3xl:gap-8 4k:gap-12 3xl:flex-1">
            <div className="flex flex-col gap-4 md:gap-5 xl:gap-6 3xl:gap-8 4k:gap-12">
              <div className="bg-[#161b22] rounded-lg shadow-lg p-4 md:p-6 xl:p-8 3xl:p-10 4k:p-16 border border-[#30363d] min-h-[250px] 3xl:flex-1 3xl:min-h-0">
                <Clocks />
              </div>
              <div className="bg-[#161b22] rounded-lg shadow-lg p-4 md:p-6 xl:p-8 3xl:p-10 4k:p-16 border border-[#30363d] min-h-[280px] 3xl:flex-1 3xl:min-h-0">
                <DagensTal />
              </div>
            </div>
            <GeneralInfo />
          </div>

          {/* Bottom Row: Holidays and Invoice Chart */}
          <div className="grid grid-rows-[auto_1fr] 3xl:grid-rows-[35%_65%] gap-4 md:gap-5 xl:gap-6 3xl:gap-8 4k:gap-12 3xl:flex-1">
            {/* Holidays Section */}
            <Holidays />

            {/* Invoice Chart */}
            <div className="bg-[#161b22] rounded-lg shadow-lg p-3 md:p-4 xl:p-6 3xl:p-8 4k:p-12 border border-[#30363d] min-h-[400px] 3xl:min-h-0">
              <InvoiceChart />
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;