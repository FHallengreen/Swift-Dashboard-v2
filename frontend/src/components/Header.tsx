import { Link } from 'react-router-dom';

interface HeaderProps {
  time: string;
  date: string;
  weekNumber: string;
}

const Header: React.FC<HeaderProps> = ({ time, date, weekNumber }) => {

  return (
    <header className="bg-[#161b22] shadow-lg border-b border-[#30363d] flex-shrink-0">
      <div className="w-full px-4 py-3 md:px-5 md:py-3.5 xl:px-6 xl:py-4 3xl:px-8 3xl:py-5 4k:px-12 4k:py-8 flex items-center justify-between">
        <div className="flex items-center gap-3 md:gap-4 xl:gap-5 3xl:gap-6 4k:gap-10">
          <Link to="/" className="flex-shrink-0">
            <img src="/images/logo.svg" alt="Swift Logo" className="h-10 md:h-12 xl:h-14 3xl:h-20 4k:h-32 w-auto" />
          </Link>
          <nav className="flex gap-1.5 md:gap-2 3xl:gap-3 4k:gap-5">
            <Link 
              to="/" 
              className="bg-[#114C96] hover:bg-[#0d3a75] px-3 py-1.5 md:px-4 md:py-2 xl:px-5 xl:py-2.5 3xl:px-7 3xl:py-3.5 4k:px-12 4k:py-6 rounded-md text-sm md:text-base xl:text-lg 3xl:text-2xl 4k:text-4xl font-semibold transition-colors text-white"
            >
              Dashboard
            </Link>
            <Link 
              to="/database" 
              className="bg-[#30363d] hover:bg-[#484f58] px-3 py-1.5 md:px-4 md:py-2 xl:px-5 xl:py-2.5 3xl:px-7 3xl:py-3.5 4k:px-12 4k:py-6 rounded-md text-sm md:text-base xl:text-lg 3xl:text-2xl 4k:text-4xl font-semibold transition-colors text-white"
            >
              Database
            </Link>
          </nav>
        </div>

        <div className="text-right">
          <h1 className="text-2xl md:text-3xl lg:text-4xl xl:text-5xl 3xl:text-7xl 4k:text-[10rem] font-bold tracking-tight leading-none text-white">
            {time}
          </h1>
          <p className="text-xs md:text-sm lg:text-base xl:text-xl 3xl:text-3xl 4k:text-6xl text-slate-300 font-medium leading-tight mt-0.5 md:mt-1 3xl:mt-2 4k:mt-4">
            {date} <span className="text-[#58a6ff]">• {weekNumber}</span>
          </p>
        </div>
      </div>
    </header>
  );
};

export default Header;