import { useState, useEffect } from 'react';
import api from '../api';

const GeneralInfo: React.FC = () => {
  const [infoText, setInfoText] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    setIsLoading(true);
    api
      .get('/info')
      .then((res) => {
        setInfoText(res.data.text || (res.data.Text || ''));
      })
      .catch((error) => {
        console.error('Error fetching info:', error);
        setError('Failed to load general information');
      })
      .finally(() => setIsLoading(false));
  }, []);

  const handleInfoChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newText = e.target.value;
    setInfoText(newText);
    setError(null);
    setSuccessMessage(null);
  };

  const handleUpdateInfo = async () => {
    setError(null);
    setSuccessMessage(null);
    try {
      await api.post('/info', { text: infoText }, {
        headers: { 'Content-Type': 'application/json' },
      });
      setSuccessMessage('Information updated successfully!');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error) {
      console.error('Error saving info:', error);
      setError('Failed to save information');
    }
  };

  return (
    <div className="h-full flex flex-col">
      <h2 className="text-3xl font-bold text-slate-200 mb-4">General Info</h2>
      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-slate-400 text-lg">Loading...</p>
        </div>
      ) : (
        <div className="flex-1 flex flex-col gap-4">
          <textarea
            className="flex-1 text-lg text-white bg-[#0d1117] border border-[#30363d] rounded-lg p-6 resize-none focus:outline-none focus:ring-2 focus:ring-[#58a6ff] focus:border-[#58a6ff] placeholder-slate-500"
            value={infoText}
            onChange={handleInfoChange}
            placeholder="Enter general information..."
          />
          <button
            onClick={handleUpdateInfo}
            className="px-6 py-3 bg-[#114C96] text-white text-lg font-bold rounded-lg hover:bg-[#0d3a75] focus:outline-none focus:ring-2 focus:ring-[#58a6ff] transition-colors"
          >
            Update
          </button>
          {error && <p className="text-center text-red-400 text-base">{error}</p>}
          {successMessage && <p className="text-center text-green-400 text-base">{successMessage}</p>}
        </div>
      )}
    </div>
  );
};

export default GeneralInfo;