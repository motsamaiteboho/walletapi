import { useEffect, useState } from "react";
import {
    getBalance,
    withdraw
} from "./api/walletApi";
import "./index.css";

function App() {
    const [balance, setBalance] = useState(null);
    const [currency, setCurrency] = useState("");
    const [amount, setAmount] = useState("");

    const [loading, setLoading] = useState(true);
    const [withdrawing, setWithdrawing] = useState(false);

    const [error, setError] = useState("");
    const [message, setMessage] = useState("");

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

    async function handleWithdraw(event) {
        event.preventDefault();

        setError("");
        setMessage("");

        const withdrawalAmount = Number(amount);

        if (!withdrawalAmount || withdrawalAmount <= 0) {
            setError(
                "Please enter a valid withdrawal amount."
            );

            return;
        }

        try {
            setWithdrawing(true);

            const data = await withdraw(
                withdrawalAmount
            );

            setBalance(data.remainingBalance);
            setCurrency(data.currency);

            setMessage(
                `Withdrawal successful. ${data.currency} ${data.withdrawnAmount.toFixed(2)} withdrawn.`
            );

            setAmount("");
        } catch (err) {
            setError(err.message);
        } finally {
            setWithdrawing(false);
        }
    }

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
                    ) : error && balance === null ? (
                        <div className="error">
                            {error}
                        </div>
                    ) : (
                        <div className="balance">
                            {currency}{" "}
                            {Number(balance).toFixed(2)}
                        </div>
                    )}
                </section>

                <form
                    className="withdraw-form"
                    onSubmit={handleWithdraw}
                >
                    <label htmlFor="amount">
                        Withdrawal Amount
                    </label>

                    <input
                        id="amount"
                        type="number"
                        min="0.01"
                        step="0.01"
                        placeholder="Enter amount"
                        value={amount}
                        onChange={(event) =>
                            setAmount(event.target.value)
                        }
                        disabled={
                            loading || withdrawing
                        }
                    />

                    <button
                        type="submit"
                        disabled={
                            loading || withdrawing
                        }
                    >
                        {withdrawing
                            ? "Processing..."
                            : "Withdraw"}
                    </button>
                </form>

                {message && (
                    <div className="success">
                        ✓ {message}
                    </div>
                )}

                {error && balance !== null && (
                    <div className="error">
                        {error}
                    </div>
                )}

            </section>
        </main>
    );
}

export default App;
