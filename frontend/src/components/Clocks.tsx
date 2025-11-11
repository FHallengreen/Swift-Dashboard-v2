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
    <div className="bg-[#0d1117] p-2 rounded-lg text-center border border-[#30363d]">
      <p className="text-xl font-semibold text-[#58a6ff] mb-2">{city}</p>
      <p className="text-5xl font-bold text-white">{timeString}</p>
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
      <h2 className="text-2xl font-bold text-slate-200 mb-4">World Clocks</h2>
      <div className="grid grid-cols-3 gap-6 flex-1 content-start">
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