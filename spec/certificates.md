# 9. Certificates (Normative)

Certificates are human- or machine-readable documents derived from Codex Entries.  
They provide verifiable evidence of custodianship, integrity, and anchoring events.  
Certificates may serve as compliance artifacts under frameworks such as UCC Section 12, eIDAS, or industry-specific regulations.

---

## 9.1 Certificate Formats

A Certificate MUST be generated in at least one of the following formats:

- **JSON Certificate**:  
  - Based directly on the Codex Entry JSON structure.  
  - Signed using [JOSE JWS].  
  - Intended for machine consumption and programmatic verification.  

- **W3C Verifiable Credential (VC)**:  
  - Conforming to the [W3C VC Data Model].  
  - Allows interoperability with decentralized identity systems.  
  - MAY include semantic extensions (e.g., JSON-LD contexts).  

- **X.509 / X.50x Certificate Binding**:  
  - An optional binding of Codex Entry metadata to an X.509v3 certificate or equivalent PKI artifact.  
  - Provides compatibility with existing TLS/PKI infrastructure.  
  - MUST use an extension field (OID) to carry the Codex Entry hash or anchor reference.  

---

## 9.2 Generation Requirements

- Certificates MUST clearly bind to a unique Codex Entry `id`.  
- Certificates MUST include:
  - Integrity proof (`storage.integrity_proof`).  
  - Anchor reference (`anchor.tx_hash`, `anchor.chain`).  
  - Signatures from authorized entities.  
- Certificates MUST reflect encryption metadata if present in the Codex Entry, and omit it if not.  
- If the Codex Entry includes a `last_controlled_by` field, it MUST be reproduced in the certificate.  
- Signatures MUST be created after anchoring and MUST cover the full Codex Entry including the anchor reference.  
- Where X.509 binding is used, the certificate MUST include the Codex Entry hash in a critical extension to prevent misuse.  

---

## 9.3 Verification Rules

Verifiers MUST:

1. Confirm the certificate format is supported and valid (JSON, VC, X.509).  
2. Validate all included signatures according to Section 6.  
3. Cross-check that the Codex Entry referenced by the certificate is itself valid (per Section 8).  
4. For X.509-bound certificates, validate the PKI chain of trust in addition to Codex Entry validation.  

If any of these checks fail, the certificate MUST be rejected.  

---

## 9.4 Revocation and Expiry

- Certificates MAY include an expiry date.  
- Certificates MAY be revoked using standard mechanisms:
  - For JSON/VC: by publishing a revocation list or status endpoint.  
  - For X.509: by CRL or OCSP.  
- Revocation MUST NOT retroactively invalidate historical Codex Entries already anchored.  

---

## 9.5 Discussion (Non-Normative)

Different certificate formats serve different operational and compliance needs:

- **JSON Certificates**  
  - Best suited for developer tooling, APIs, and automated verification.  
  - Lightweight and easy to parse in any environment.  
  - Limited interoperability with identity frameworks outside JSON ecosystems.  
  - Certificates MUST conform to Codex Entry schema rules for optional fields like encryption and identity.

- **W3C Verifiable Credentials (VCs)**  
  - Ideal for decentralized identity, self-sovereign identity (SSI), and Web3 systems.  
  - Strong interoperability with DID methods and JSON-LD vocabularies.  
  - Heavier processing requirements due to JSON-LD and semantic validation.  
  - Certificates MUST conform to Codex Entry schema rules for optional fields like encryption and identity.

- **X.509 Certificates**  
  - Appropriate when integration with existing TLS/PKI infrastructure is required (e.g., corporate IT, HTTPS, VPNs).  
  - Provides backward compatibility with regulators, enterprises, and legacy compliance systems.  
  - Heavier reliance on centralized certificate authorities (CAs) compared to JSON/VC approaches.  

### Choosing a format

- Use **JSON Certificates** when working with APIs, developer tools, and blockchain-native systems.  
- Use **VCs** when operating in ecosystems where decentralized identity and SSI are priorities.  
- Use **X.509 bindings** when bridging to traditional IT or regulatory systems that already mandate PKI.  

Implementers MAY support multiple formats in parallel, allowing the same Codex Entry to generate different certificates depending on the target environment or audience.

---

## 9.6 Example Flow (Non-Normative)

A single Codex Entry can generate multiple certificates for different consumers:

```
            +-----------------+
            |   Codex Entry   |
            |  (signed JSON)  |
            +--------+--------+
                     |
     ----------------+----------------
     |                               |
+----v----+                     +----v----+
|  JSON   |                     |   VC    |
|Certificate|                   |Verifiable|
|(JWS)     |                     |Credential|
+----+----+                     +----+----+
     |                               |
 For APIs, CLI tools,        For decentralized ID
 and blockchain-native        ecosystems, SSI,
 systems.                    and Web3 workflows.
     
     
               +-----------------+
               |     X.509       |
               |   Certificate   |
               |   (with OID)    |
               +--------+--------+
                        |
               For traditional PKI,
               TLS/HTTPS, VPNs,
               and enterprise IT.
```

---

## 9.7 Example JSON Certificate (Non-Normative)

```json
{
  "certificate_type": "lockb0x-json",
  "codex_entry_id": "550e8400-e29b-41d4-a716-446655440000",
  "protocol_version": "1.0.0",
  "issued_at": "2025-09-15T12:00:00Z",
  "issuer": "did:example:org123",
  "subject": "did:example:asset789",
  "identity": {
    "org": "Example Org",
    "process": "Lockb0x",
    "artifact": "Enterprise Asset Management"
  },
  "encryption": {
    "algorithm": "AES-256-GCM",
    "key_ownership": "did:example:org123#key-1",
    "last_controlled_by": "did:example:user456"
  },
  "storage": {
    "protocol": "gcs",
    "integrity_proof": "ni:///sha-256;4nq34kaf9djf23...",
    "media_type": "application/pdf",
    "size_bytes": 204800,
    "location": {
      "region": "us-central1",
      "jurisdiction": "US/CA",
      "provider": "Google Cloud"
    }
  },
  "anchor": {
    "chain": "stellar:pubnet",
    "tx_hash": "abcdef123456...",
    "hash_alg": "SHA256"
  },
  "signatures": [
    {
      "protected": {
        "alg": "EdDSA",
        "kid": "stellar:GA123..."
      },
      "signature": "MEUCIQD0vE..."
    }
  ]
}
```

---

## 9.8 Example W3C Verifiable Credential (Non-Normative)

```json
{
  "@context": [
    "https://www.w3.org/2018/credentials/v1",
    "https://w3id.org/security/suites/ed25519-2020/v1",
    "https://example.org/lockb0x/v1"
  ],
  "id": "urn:uuid:550e8400-e29b-41d4-a716-446655440000",
  "type": ["VerifiableCredential", "Lockb0xCertificate"],
  "issuer": "did:example:org123",
  "issuanceDate": "2025-09-15T12:00:00Z",
  "credentialSubject": {
    "id": "did:example:asset789",
    "identity": {
      "org": "Example Org",
      "process": "Lockb0x",
      "artifact": "Enterprise Asset Management"
    },
    "encryption": {
      "algorithm": "AES-256-GCM",
      "keyOwnership": "did:example:org123#key-1",
      "lastControlledBy": "did:example:user456"
    },
    "codexEntry": {
      "protocolVersion": "1.0.0",
      "storage": {
        "protocol": "gcs",
        "integrityProof": "ni:///sha-256;4nq34kaf9djf23...",
        "mediaType": "application/pdf",
        "sizeBytes": 204800,
        "location": {
          "region": "us-central1",
          "jurisdiction": "US/CA",
          "provider": "Google Cloud"
        }
      },
      "anchor": {
        "chain": "stellar:pubnet",
        "txHash": "abcdef123456...",
        "hashAlg": "SHA256"
      }
    }
  },
  "proof": {
    "type": "Ed25519Signature2020",
    "created": "2025-09-15T12:00:00Z",
    "verificationMethod": "did:example:org123#keys-1",
    "proofPurpose": "assertionMethod",
    "jws": "eyJhbGciOiJFZERTQSJ9..."
  }
}
```

---

## 9.9 Example X.509 Binding (Non-Normative)

```
Certificate:
    Data:
        Version: 3 (0x2)
        Serial Number: 123456789 (0x75bcd15)
        Signature Algorithm: sha256WithRSAEncryption
        Issuer: CN=Example CA, O=Example Org, C=US
        Validity
            Not Before: Sep 15 00:00:00 2025 GMT
            Not After : Sep 15 00:00:00 2026 GMT
        Subject: CN=Lockb0x Asset, O=Example Org, C=US
        Subject Public Key Info:
            Public Key Algorithm: rsaEncryption
                Public-Key: (2048 bit)
        X509v3 extensions:
            X509v3 Basic Constraints: critical
                CA:FALSE
            X509v3 Key Usage: critical
                Digital Signature, Non Repudiation
            X509v3 Subject Alternative Name:
                DNS:lockb0x.example.org
            1.3.6.1.4.1.55555.1.1 (Lockb0x Codex Entry OID): critical
                CodexEntryHash=ni:///sha-256;4nq34kaf9djf23...
                AnchorChain=stellar:pubnet
                AnchorTxHash=abcdef123456...
    Signature Algorithm: sha256WithRSAEncryption
         12:34:56:...
```

---

[JOSE JWS]: https://www.rfc-editor.org/rfc/rfc7515  
[W3C VC Data Model]: https://www.w3.org/TR/vc-data-model/
