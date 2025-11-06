<svg xmlns="http://www.w3.org/2000/svg" width="720" height="460" font-family="Segoe UI,Helvetica,Arial,sans-serif" font-size="13">
  <rect x="220" y="20" width="280" height="60" rx="8" ry="8" fill="#f8fafc" stroke="#4b5563"/>
  <text x="360" y="48" text-anchor="middle" font-weight="600">Lockb0x Codex</text>
  <text x="360" y="66" text-anchor="middle" fill="#475569">(canonical JSON envelope, ni)</text>

  <!-- Google -->
  <rect x="40" y="140" width="180" height="100" rx="8" ry="8" fill="#e8f5e9" stroke="#34a853"/>
  <text x="130" y="160" text-anchor="middle" font-weight="600">Google Anchor</text>
  <text x="130" y="178" text-anchor="middle">OIDC ID Token • Drive JWS</text>
  <text x="130" y="196" text-anchor="middle">WebAuthn Signature</text>

  <!-- Ethereum -->
  <rect x="270" y="140" width="180" height="100" rx="8" ry="8" fill="#f3e8ff" stroke="#7e22ce"/>
  <text x="360" y="160" text-anchor="middle" font-weight="600">Ethereum Anchor</text>
  <text x="360" y="178" text-anchor="middle">EIP-4361 • IPFS/Filecoin</text>
  <text x="360" y="196" text-anchor="middle">Wallet Signature</text>

  <!-- Microsoft -->
  <rect x="500" y="140" width="180" height="100" rx="8" ry="8" fill="#e0f2fe" stroke="#2563eb"/>
  <text x="590" y="160" text-anchor="middle" font-weight="600">Microsoft Anchor</text>
  <text x="590" y="178" text-anchor="middle">Entra ID Token • OneDrive JWS</text>
  <text x="590" y="196" text-anchor="middle">WebAuthn Signature</text>

  <!-- Validator -->
  <rect x="140" y="280" width="440" height="90" rx="8" ry="8" fill="#fefce8" stroke="#ca8a04"/>
  <text x="360" y="302" text-anchor="middle" font-weight="600">Lockb0x Codex Validator</text>
  <text x="360" y="320" text-anchor="middle">Schema + Signature Validation + Cross-Anchor Consistency</text>

  <!-- Attestation -->
  <rect x="260" y="400" width="200" height="45" rx="8" ry="8" fill="#eef2ff" stroke="#4338ca"/>
  <text x="360" y="428" text-anchor="middle" font-weight="600">Lockb0x Attestation (JWS/on-chain)</text>

  <!-- arrows -->
  <path d="M360 80v20M130 120v20M360 120v20M590 120v20" stroke="#4b5563" stroke-width="2" marker-end="url(#arrow)"/>
  <path d="M130 240v40M360 240v40M590 240v40" stroke="#4b5563" stroke-width="2" marker-end="url(#arrow)"/>
  <path d="M360 370v30" stroke="#4b5563" stroke-width="2" marker-end="url(#arrow)"/>

  <defs>
    <marker id="arrow" markerWidth="10" markerHeight="10" refX="6" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L6,3 z" fill="#4b5563"/>
    </marker>
  </defs>
</svg>
