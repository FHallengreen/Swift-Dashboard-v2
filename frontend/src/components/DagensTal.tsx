import { useEffect, useState } from "react";
import api from "../api";
import * as signalR from "@microsoft/signalr";

interface Invoice {
  year: number;
  month: number;
  amount: number;
}

const danishDecimalDisplayFormat = new Intl.NumberFormat("da-DK", {
  style: "decimal",
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const DagensTal: React.FC = () => {
  const [currentInvoice, setCurrentInvoice] = useState<Invoice>({
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    amount: 0,
  });

  const [draft, setDraft] = useState<string>(""); // user input
  const [isEditing, setIsEditing] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCurrentInvoice = async () => {
      setIsLoading(true);
      try {
        const res = await api.get("/invoices/current");
        setCurrentInvoice(res.data);
      } catch (err) {
        console.error("Error fetching current invoice:", err);
        setError("Failed to load Dagens Tal");
      } finally {
        setIsLoading(false);
      }
    };

    fetchCurrentInvoice();

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/api/invoiceHub")
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveInvoiceUpdate", (data: Invoice) => {
      const today = new Date();
      if (
        data.year === today.getFullYear() &&
        data.month === today.getMonth() + 1
      ) {
        setCurrentInvoice(data);

        // Do NOT overwrite user input while editing
        if (!isEditing) {
          setDraft(danishDecimalDisplayFormat.format(data.amount));
        }
      }
    });

    connection
      .start()
      .then(() => console.log("SignalR connected"))
      .catch((err) => console.error("SignalR error:", err));

    return () => {
      connection.stop();
    };
  }, [isEditing]);

  const parseDaDecimal = (value: string): number | null => {
    if (!value.trim()) return null;

    // Remove spaces
    let s = value.replace(/\s/g, "");

    // Find last occurrence of . or ,
    const lastDot = s.lastIndexOf(".");
    const lastComma = s.lastIndexOf(",");

    let decimalSeparator = "";

    if (lastDot > lastComma) decimalSeparator = ".";
    else if (lastComma > lastDot) decimalSeparator = ",";

    if (decimalSeparator) {
      const parts = s.split(decimalSeparator);
      const integerPart = parts[0].replace(/[.,]/g, "");
      const decimalPart = parts[1] ?? "";
      s = `${integerPart}.${decimalPart}`;
    } else {
      // No decimal separator → whole number
      s = s.replace(/[^\d]/g, "");
    }

    const num = Number(s);
    return Number.isFinite(num) ? num : null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const parsed = parseDaDecimal(draft);
    if (parsed === null) {
      setError("Please enter a valid number");
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      await api.post("/invoices", parsed, {
        headers: { "Content-Type": "application/json" },
      });

      setCurrentInvoice({ ...currentInvoice, amount: parsed });
      setIsEditing(false);
    } catch {
      setError("Failed to update Dagens Tal");
    } finally {
      setIsLoading(false);
    }
  };

  const formattedAmount = `${danishDecimalDisplayFormat.format(
    currentInvoice.amount
  )} EUR`;

  return (
    <div className="h-full flex flex-col">
      <h2 className="text-2xl md:text-3xl xl:text-4xl 3xl:text-6xl 4k:text-8xl font-bold text-slate-200 mb-3">
        Dagens Tal
      </h2>

      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-slate-400 text-lg">Loading...</p>
        </div>
      ) : error ? (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-red-400 text-lg">{error}</p>
        </div>
      ) : (
        <div className="flex-1 flex flex-col items-center justify-center gap-4">
          {isEditing ? (
            <form
              onSubmit={handleSubmit}
              className="w-full flex flex-col gap-4"
            >
              <input
                type="text"
                value={draft}
                onChange={(e) =>
                  setDraft(e.target.value.replace(/[^\d.,]/g, ""))
                }
                autoFocus
                className="text-4xl font-bold text-center border-2 border-[#58a6ff] rounded-md p-3 bg-[#0d1117] text-white focus:outline-none focus:ring-2 focus:ring-[#58a6ff]"
                data-testid="invoice-input"
              />

              <div className="flex gap-3">
                <button
                  type="submit"
                  className="flex-1 bg-[#114C96] text-white font-semibold py-3 rounded-md hover:bg-[#0d3a75]"
                  data-testid="invoice-submit"
                >
                  Save
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setIsEditing(false);
                    setError(null);
                  }}
                  className="flex-1 bg-[#30363d] text-white font-semibold py-3 rounded-md hover:bg-[#484f58]"
                >
                  Cancel
                </button>
              </div>
            </form>
          ) : (
            <button
              onClick={() => {
                setDraft(
                  danishDecimalDisplayFormat.format(currentInvoice.amount)
                );
                setIsEditing(true);
              }}
              className="text-6xl font-bold text-white hover:text-[#58a6ff] transition-colors"
              data-testid="invoice-amount"
            >
              {formattedAmount}
            </button>
          )}
        </div>
      )}
    </div>
  );
};

export default DagensTal;
