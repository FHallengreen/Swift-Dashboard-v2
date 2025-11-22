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

    connection.on("ReceiveInfoUpdate", (data: { Text?: string; text?: string }) => {
      console.log("SignalR: ReceiveInfoUpdate", data);
      const newText = data.Text ?? data.text ?? '';
      setInfoText(newText);
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
    const textToSave = infoText;
    try {
      await api.post('/info', { text: textToSave }, {
        headers: { 'Content-Type': 'application/json' },
      });
      // Keep the text in state after successful save
      setInfoText(textToSave);
      setSuccessMessage('Information updated successfully!');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error) {
      console.error('Error saving info:', error);
      setError('Failed to save information');
    }
  };

  return (
    <div className="bg-[#161b22] rounded-lg shadow-lg p-3 md:p-4 xl:p-4 3xl:p-6 4k:p-10 border border-[#30363d] h-full flex flex-col">
      <h2 className="text-2xl md:text-3xl xl:text-4xl 3xl:text-6xl 4k:text-8xl font-bold text-slate-200 mb-3 md:mb-4 xl:mb-5 3xl:mb-8 4k:mb-10">General Info</h2>
      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-slate-400 text-lg md:text-xl xl:text-xl 3xl:text-2xl 4k:text-4xl">Loading...</p>
        </div>
      ) : (
        <div className="flex-1 flex flex-col gap-3 md:gap-3.5 xl:gap-3.5 3xl:gap-4 4k:gap-6">
          <textarea
            className="flex-1 text-lg md:text-2xl xl:text-3xl 3xl:text-5xl 4k:text-7xl text-white bg-[#0d1117] border border-[#30363d] rounded-lg p-3 md:p-4 xl:p-5 3xl:p-8 4k:p-12 resize-none focus:outline-none focus:ring-2 focus:ring-[#58a6ff] focus:border-[#58a6ff] placeholder-slate-500 leading-relaxed"
            value={infoText}
            onChange={handleInfoChange}
            placeholder="Enter general information..."
          />
          <button
            onClick={handleUpdateInfo}
            className="px-4 py-2 md:px-6 md:py-2.5 xl:px-7 xl:py-3 3xl:px-10 3xl:py-4 4k:px-16 4k:py-8 bg-[#114C96] text-white text-base md:text-lg xl:text-2xl 3xl:text-3xl 4k:text-5xl font-bold rounded-lg hover:bg-[#0d3a75] focus:outline-none focus:ring-2 focus:ring-[#58a6ff] transition-colors"
          >
            Update
          </button>
          {error && <p className="text-center text-red-400 text-sm md:text-base xl:text-base 3xl:text-lg 4k:text-2xl">{error}</p>}
          {successMessage && <p className="text-center text-green-400 text-sm md:text-base xl:text-base 3xl:text-lg 4k:text-2xl">{successMessage}</p>}
        </div>
      )}
    </div>
  );
};

export default GeneralInfo;