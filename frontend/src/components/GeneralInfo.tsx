import { useState, useEffect } from 'react';
import api from '../api';
import * as signalR from '@microsoft/signalr';

const GeneralInfo: React.FC = () => {
  const [infoText, setInfoText] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    const fetchInfo = () => {
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
    };

    fetchInfo();

    // Set up SignalR connection
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/api/infoHub')
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveInfoUpdate", (data: { Text: string }) => {
      console.log("SignalR: ReceiveInfoUpdate", data);
      setInfoText(data.Text || '');
    });

    connection.start()
      .then(() => console.log('SignalR Connected for GeneralInfo'))
      .catch(err => console.error('SignalR Connection Error: ', err));

    return () => {
      connection.stop();
    };
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
    <div className="bg-[#161b22] rounded-lg shadow-lg p-3 md:p-4 xl:p-4 3xl:p-6 border border-[#30363d] h-full flex flex-col">
      <h2 className="text-2xl md:text-3xl xl:text-3xl 3xl:text-4xl font-bold text-slate-200 mb-3 md:mb-4 xl:mb-4 3xl:mb-6">General Info</h2>
      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-slate-400 text-lg md:text-xl xl:text-xl 3xl:text-2xl">Loading...</p>
        </div>
      ) : (
        <div className="flex-1 flex flex-col gap-3 md:gap-3.5 xl:gap-3.5 3xl:gap-4">
          <textarea
            className="flex-1 text-lg md:text-2xl xl:text-2xl 3xl:text-3xl text-white bg-[#0d1117] border border-[#30363d] rounded-lg p-3 md:p-4 xl:p-4 3xl:p-6 resize-none focus:outline-none focus:ring-2 focus:ring-[#58a6ff] focus:border-[#58a6ff] placeholder-slate-500 leading-relaxed"
            value={infoText}
            onChange={handleInfoChange}
            placeholder="Enter general information..."
          />
          <button
            onClick={handleUpdateInfo}
            className="px-4 py-2 md:px-6 md:py-2.5 xl:px-6 xl:py-2.5 3xl:px-8 3xl:py-3 bg-[#114C96] text-white text-base md:text-lg xl:text-lg 3xl:text-xl font-bold rounded-lg hover:bg-[#0d3a75] focus:outline-none focus:ring-2 focus:ring-[#58a6ff] transition-colors"
          >
            Update
          </button>
          {error && <p className="text-center text-red-400 text-sm md:text-base xl:text-base 3xl:text-lg">{error}</p>}
          {successMessage && <p className="text-center text-green-400 text-sm md:text-base xl:text-base 3xl:text-lg">{successMessage}</p>}
        </div>
      )}
    </div>
  );
};

export default GeneralInfo;