const API_BASE_URL =
    import.meta.env.VITE_API_BASE_URL;

const WALLET_ID =
    "11111111-1111-1111-1111-111111111111";

export async function getBalance() {
    const response = await fetch(
        `${API_BASE_URL}/api/wallet-accounts/${WALLET_ID}/balance`
    );

    if (!response.ok) {
        throw new Error(
            `Failed to retrieve wallet balance (${response.status})`
        );
    }

    return response.json();
}