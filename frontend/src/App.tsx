import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Dashboard from './components/Dashboard';
import DatabaseView from './components/DatabaseView';

const App: React.FC = () => {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/database" element={<DatabaseView />} />
      </Routes>
    </Router>
  );
};

export default App;