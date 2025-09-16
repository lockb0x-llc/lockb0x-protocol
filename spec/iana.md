


# 13. IANA Considerations (Normative)

This section describes interactions with existing IANA registries.  
At this time, the Lockb0x Protocol does not require new IANA registries but relies on existing ones.

---

## 13.1 Media Types

- Implementations MUST use registered IANA media types ([RFC 6838]) for declaring file formats.  
- New media types SHOULD be registered with IANA if required for extensions.  

---

## 13.2 JOSE / COSE Algorithms

- Signature algorithms MUST reference the IANA JSON Web Signature and Encryption (JWS/JWE) registries ([RFC 7515], [RFC 8152]).  
- Implementations MUST NOT invent ad-hoc identifiers for cryptographic algorithms.  

---

## 13.3 CAIP Identifiers

- Blockchain references MUST conform to [CAIP-2] for chain IDs and [CAIP-10] for account identifiers.  
- No new identifier scheme is required at this time.  

---

## 13.4 Future Lockb0x OID Arc (X.509)

- The X.509 binding MAY require a dedicated Lockb0x OID arc for Codex Entry extensions.  
- This specification does not define such an arc but recommends reserving one under an IANA Private Enterprise Number (PEN).  
- Future versions of this specification MAY define formal OIDs for Lockb0x extensions.  

---

## 13.5 Summary

- No new IANA actions are required for this version of the specification.  
- Lockb0x leverages existing IANA registries for interoperability.  
- Future updates MAY request dedicated OIDs or media type registrations.

---

[RFC 6838]: https://www.rfc-editor.org/rfc/rfc6838  
[RFC 7515]: https://www.rfc-editor.org/rfc/rfc7515  
[RFC 8152]: https://www.rfc-editor.org/rfc/rfc8152  
[CAIP-2]: https://github.com/ChainAgnostic/CAIPs/blob/master/CAIPs/caip-2.md  
[CAIP-10]: https://github.com/ChainAgnostic/CAIPs/blob/master/CAIPs/caip-10.md