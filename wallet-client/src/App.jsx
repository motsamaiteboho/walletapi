import { useEffect, useState } from "react";
import './App.css'
import "./index.css";
import { getBalance } from "./api/walletApi";

function App() {
    const [balance, setBalance] = useState(null);
    const [currency, setCurrency] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        async function loadWallet() {
            try {
                setLoading(true);
                setError("");

                const data = await getBalance();

                setBalance(data.balance);
                setCurrency(data.currency);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        }

        loadWallet();
    }, []);

    return (
        <main className="app">
            <section className="wallet-card">
                <header className="wallet-header">
                    <h1>My Wallet</h1>
                    <p>Manage your wallet balance</p>
                </header>

                <section className="balance-section">
                    <span className="balance-label">
                        Current Balance
                    </span>

                    {loading ? (
                        <div className="loading">
                            Loading balance...
                        </div>
                    ) : error ? (
                        <div className="error">
                            {error}
                        </div>
                    ) : (
                        <>
                            <div className="balance">
                                {currency}{" "}
                                {Number(balance).toFixed(2)}
                            </div>
                        </>
                    )}
                </section>

                <form className="withdraw-form">
                    <label htmlFor="amount">
                        Withdrawal Amount
                    </label>

                    <input
                        id="amount"
                        type="number"
                        min="0.01"
                        step="0.01"
                        placeholder="Enter amount"
                    />

                    <button type="submit">
                        Withdraw
                    </button>
                </form>
            </section>
        </main>
    );
}

export default App;
