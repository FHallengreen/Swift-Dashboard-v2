import { Link } from 'react-router-dom';

interface HeaderProps {
  time: string;
  date: string;
  weekNumber: string;
}

const Header: React.FC<HeaderProps> = ({ time, date, weekNumber }) => {

  return (
    <header className="bg-[#161b22] shadow-lg border-b border-[#30363d] flex-shrink-0">
      <div className="w-full px-6 py-4 flex items-center justify-between">
        <div className="flex items-center gap-5">
          <Link to="/" className="flex-shrink-0">
            <img src="/images/logo.svg" alt="Swift Logo" className="h-14 w-auto" />
          </Link>
          <nav className="flex gap-2">
            <Link 
              to="/" 
              className="bg-[#114C96] hover:bg-[#0d3a75] px-5 py-2.5 rounded-md text-base font-semibold transition-colors text-white"
            >
              Dashboard
            </Link>
            <Link 
              to="/database" 
              className="bg-[#30363d] hover:bg-[#484f58] px-5 py-2.5 rounded-md text-base font-semibold transition-colors text-white"
            >
              Database
            </Link>
          </nav>
        </div>

        <div className="text-right">
          <h1 className="text-5xl font-bold tracking-tight leading-none text-white">
            {time}
          </h1>
          <p className="text-lg text-slate-300 font-medium leading-tight mt-1">
            {date} <span className="text-[#58a6ff]">• {weekNumber}</span>
          </p>
        </div>
      </div>
    </header>
  );
};

export default Header;