import { useState, useEffect } from 'react';

interface ClockInfo {
  city: string;
  timezone: string;
}

const initialClocks: ClockInfo[] = [
  { city: 'Los Angeles', timezone: 'America/Los_Angeles' },
  { city: 'Athens', timezone: 'Europe/Athens' },
  { city: 'Singapore', timezone: 'Asia/Singapore' },
  { city: 'Panama', timezone: 'America/Panama' },
  { city: 'Dubai', timezone: 'Asia/Dubai' },
  { city: 'Tokyo/Incheon', timezone: 'Asia/Tokyo' },
  { city: 'Houston', timezone: 'America/Chicago' },
  { city: 'Mumbai', timezone: 'Asia/Kolkata' },
  { city: 'Sydney', timezone: 'Australia/Sydney' },
];

interface ClockDisplayProps extends ClockInfo {
  currentTime: Date;
}

const ClockDisplay: React.FC<ClockDisplayProps> = ({ city, timezone, currentTime }) => {
  const timeString = currentTime.toLocaleTimeString('en-GB', {
    timeZone: timezone,
    hour: '2-digit',
    minute: '2-digit',
  });

  return (
    <div className="bg-[#0d1117] p-2 md:p-3 xl:p-4 3xl:p-6 4k:p-10 rounded-lg text-center border border-[#30363d]">
      <p className="text-sm md:text-lg xl:text-2xl 3xl:text-4xl 4k:text-6xl font-semibold text-[#58a6ff] mb-1 md:mb-2 xl:mb-3 3xl:mb-4 4k:mb-6">{city}</p>
      <p className="text-2xl md:text-4xl lg:text-5xl xl:text-6xl 3xl:text-8xl 4k:text-9xl font-bold text-white">{timeString}</p>
    </div>
  );
};

const Clocks: React.FC = () => {
  const [now, setNow] = useState(new Date());

  useEffect(() => {
    const timerId = setInterval(() => {
      setNow(new Date());
    }, 1000);

    return () => clearInterval(timerId);
  }, []);

  return (
    <div className="h-full flex flex-col">
      <h2 className="text-2xl md:text-3xl xl:text-4xl 3xl:text-6xl 4k:text-8xl font-bold text-slate-200 mb-3 md:mb-4 xl:mb-5 3xl:mb-8 4k:mb-10">World Clocks</h2>
      <div className="grid grid-cols-2 md:grid-cols-3 gap-3 md:gap-4 xl:gap-5 3xl:gap-8 4k:gap-10 flex-1 content-start">
        {initialClocks.map((clock) => (
          <ClockDisplay
            key={clock.city}
            city={clock.city}
            timezone={clock.timezone}
            currentTime={now}
          />
        ))}
      </div>
    </div>
  );
};

export default Clocks;