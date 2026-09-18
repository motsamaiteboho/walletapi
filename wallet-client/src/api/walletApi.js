const API_BASE_URL =
    import.meta.env.VITE_API_BASE_URL;

const WALLET_ID =
    "11111111-1111-1111-1111-111111111111";

async function handleResponse(response) {
    const data = await response.json();

    if (!response.ok) {
        throw new Error(
            data.detail ||
            data.title ||
            "An unexpected error occurred."
        );
    }

    return data;
}

export async function getBalance() {
    const response = await fetch(
        `${API_BASE_URL}/api/wallet-accounts/${WALLET_ID}/balance`
    );

    return handleResponse(response);
}

export async function withdraw(
    amount,
    idempotencyKey
) {
    const response = await fetch(
        `${API_BASE_URL}/api/wallet-accounts/${WALLET_ID}/withdraw`,
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Idempotency-Key": idempotencyKey
            },
            body: JSON.stringify({
                amount: Number(amount)
            })
        }
    );

    return handleResponse(response);
}