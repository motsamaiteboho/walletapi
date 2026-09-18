import { useCallback, useEffect, useState } from "react";
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
    const [refreshing, setRefreshing] = useState(false);

    const [error, setError] = useState("");
    const [message, setMessage] = useState("");

    /*
     * Load the current wallet balance from the API.
     *
     * useCallback allows the same function to be used by:
     * - the initial page load
     * - the Refresh Balance button
     */
    const loadWallet = useCallback(async () => {
        try {
            setError("");

            const data = await getBalance();

            setBalance(data.balance);
            setCurrency(data.currency);
        } catch (err) {
            setError(
                err.message || "Unable to load wallet balance."
            );
        }
    }, []);

    /*
     * Load wallet when the application starts.
     */
    useEffect(() => {
        async function initialiseWallet() {
            try {
                setLoading(true);
                await loadWallet();
            } finally {
                setLoading(false);
            }
        }

        initialiseWallet();
    }, [loadWallet]);

    /*
     * Refresh the wallet balance from the backend.
     */
    async function handleRefresh() {
        try {
            setRefreshing(true);
            setError("");
            setMessage("");

            await loadWallet();
        } finally {
            setRefreshing(false);
        }
    }

    /*
     * Process a withdrawal.
     */
    async function handleWithdraw(event) {
        event.preventDefault();

        setError("");
        setMessage("");

        const withdrawalAmount = Number(amount);

        /*
         * Client-side validation.
         */
        if (!amount || withdrawalAmount <= 0) {
            setError(
                "Please enter a valid withdrawal amount."
            );
            return;
        }

        /*
         * Generate a unique idempotency key for this
         * withdrawal operation.
         */
        const idempotencyKey = crypto.randomUUID();

        try {
            setWithdrawing(true);

            const data = await withdraw(
                withdrawalAmount,
                idempotencyKey
            );

            /*
             * Update the UI using the authoritative response
             * returned by the backend.
             */
            setBalance(data.remainingBalance);
            setCurrency(data.currency);

            setMessage(
                `Withdrawal successful. ${data.currency} ${Number(
                    data.withdrawnAmount
                ).toFixed(2)} withdrawn.`
            );

            /*
             * Clear the input after a successful withdrawal.
             */
            setAmount("");
        } catch (err) {
            setError(
                err.message || "Unable to process withdrawal."
            );
        } finally {
            setWithdrawing(false);
        }
    }

    return (
        <main className="app">
            <section className="wallet-card">

                {/* Header */}
                <header className="wallet-header">
                    <h1>My Wallet</h1>
                    <p>Manage your wallet balance</p>
                </header>

                {/* Balance */}
                <section className="balance-section">
                    <span className="balance-label">
                        Current Balance
                    </span>

                    {loading ? (
                        <div className="loading">
                            Loading balance...
                        </div>
                    ) : balance === null ? (
                        <div className="error">
                            {error || "Unable to load balance."}
                        </div>
                    ) : (
                        <div className="balance">
                            {currency}{" "}
                            {Number(balance).toFixed(2)}
                        </div>
                    )}

                    {/* Refresh */}
                    {!loading && (
                        <button
                            type="button"
                            className="refresh-button"
                            onClick={handleRefresh}
                            disabled={refreshing || withdrawing}
                        >
                            {refreshing
                                ? "Refreshing..."
                                : "Refresh Balance"}
                        </button>
                    )}
                </section>

                {/* Withdrawal form */}
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
                            loading ||
                            withdrawing ||
                            balance === null
                        }
                    />

                    <button
                        type="submit"
                        disabled={
                            loading ||
                            withdrawing ||
                            balance === null
                        }
                    >
                        {withdrawing
                            ? "Processing..."
                            : "Withdraw"}
                    </button>
                </form>

                {/* Success message */}
                {message && (
                    <div className="success">
                        ✓ {message}
                    </div>
                )}

                {/* Error message */}
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