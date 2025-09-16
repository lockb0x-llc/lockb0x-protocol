


# Appendix C. Security Threat Models (Non-Normative)

This appendix outlines potential threats to the Lockb0x Protocol and recommended mitigations.  
It complements Section 11 (Security Considerations) by providing more detailed scenarios and adversarial models.

---

## C.1 Key Compromise

**Threat**: An attacker gains access to a private key used for signing or encryption.  
**Impact**: Unauthorized Codex Entries, forged certificates, or decryption of sensitive data.  
**Mitigations**:
- Require hardware security modules (HSMs) or secure enclaves for key storage.  
- Use multi-signature policies to distribute trust.  
- Enforce key rotation and revocation policies.  
- Monitor anchors for unusual signing patterns.  

---

## C.2 Malicious Storage Provider

**Threat**: A storage provider tampers with or withholds stored data.  
**Impact**: Data integrity is broken or access is denied.  
**Mitigations**:
- Enforce client-side encryption so providers never see plaintext.  
- Require integrity proofs (RFC 6920 ni-URIs).  
- Support redundant storage backends (multi-adapter strategies).  
- Track storage location metadata for accountability.  

---

## C.3 Replay Attacks

**Threat**: A valid Codex Entry or certificate is replayed in a new context.  
**Impact**: Fraudulent reuse of valid data.  
**Mitigations**:
- Require unique transaction hashes for anchors.  
- Enforce canonicalization of JSON (RFC 8785).  
- Use timestamps and nonces where appropriate.  
- Reject duplicate or conflicting anchors.  

---

## C.4 Hash Collisions

**Threat**: An attacker produces two files with the same integrity proof.  
**Impact**: A malicious file is substituted for a legitimate one.  
**Mitigations**:
- Mandate collision-resistant algorithms (SHA-256, SHA3).  
- Prohibit truncated hashes.  
- Plan for migration to post-quantum hashes when available.  

---

## C.5 False Jurisdiction Claims

**Threat**: An entity claims storage or anchoring in a jurisdiction it does not control.  
**Impact**: Regulatory or compliance violations.  
**Mitigations**:
- Require jurisdiction metadata to be cryptographically anchored.  
- Bind certificates to jurisdiction claims.  
- Perform audits against blockchain anchors.  

---

## C.6 Insider Threats

**Threat**: Authorized insiders misuse their signing or encryption privileges.  
**Impact**: Data leaks, forged Codex Entries, or unauthorized releases.  
**Mitigations**:
- Enforce separation of duties with multi-sig.  
- Log all signing and anchoring events with tamper-proof audit trails.  
- Rotate personnel keys and disable access upon role change or termination.  

---

## C.7 Denial of Service (DoS)

**Threat**: Attackers flood verifiers or anchors with excessive entries.  
**Impact**: Service degradation or anchor network congestion.  
**Mitigations**:
- Rate-limit Codex Entry submissions.  
- Cache verified certificates.  
- Use decentralized anchors with high throughput (e.g., Stellar).  
- Monitor for abuse patterns.  

---

## C.8 Post-Quantum Threats

**Threat**: Quantum computing renders current cryptographic algorithms insecure.  
**Impact**: Integrity proofs and signatures may be broken.  
**Mitigations**:
- Design protocol to be crypto-agile (support new algorithms without breaking schema).  
- Track NIST post-quantum standardization.  
- Plan migration strategies for certificates, signatures, and integrity proofs.  

---

These threat models are illustrative. Implementers MUST adapt mitigations based on risk profiles, regulatory environments, and operational contexts.