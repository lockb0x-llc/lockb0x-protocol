{
"$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.lockb0x.io/provider-descriptor.schema.json",
"title": "Lockb0x Provider Descriptor Schema",
"description": "Schema for self-describing provider descriptor documents used by Lockb0x implementations.",
"type": "object",
"required": [
"providerId",
"providerUri",
"version",
"name",
"identityScheme",
"storageScheme",
"algorithms"
],
"properties": {
"providerId": {
"type": "string",
"pattern": "^[a-z0-9\\-]+$",
      "description": "Short lowercase identifier for this provider (e.g., 'google', 'ethereum')."
    },
    "providerUri": {
      "type": "string",
      "format": "uri",
      "description": "Canonical URI or decentralized identifier for this provider descriptor (e.g., 'https://provider.example.com/lockb0x.json', 'ipfs://...', 'did:web:example.com')."
    },
    "version": {
      "type": "string",
      "pattern": "^v[0-9]+(\\.[0-9]+)?$",
"description": "Version of the provider descriptor."
},
"name": {
"type": "string",
"description": "Human-readable name of the provider or ecosystem."
},
"description": {
"type": "string",
"description": "Optional longer text explaining the ecosystem context and intended use."
},
"ecosystem": {
"type": "string",
"description": "Optional label for the ecosystem family (e.g., 'Google Cloud', 'EVM', 'Substrate', 'Microsoft Graph')."
},
"identityScheme": {
"type": "object",
"required": ["type", "format"],
"properties": {
"type": {
"type": "string",
"description": "Identity protocol or credential type (e.g., 'oidc', 'eip-4361', 'sr25519')."
},
"format": {
"type": "string",
"description": "Signature container format (e.g., 'jws', 'cose', 'raw')."
},
"algorithms": {
"type": "array",
"items": { "type": "string" },
"description": "Supported signing algorithms for identity (e.g., ['RS256', 'ES256'])."
},
"issuer": {
"type": "string",
"description": "Optional canonical issuer domain or DID for this identity source."
}
}
},
"storageScheme": {
"type": "object",
"required": ["type", "format"],
"properties": {
"type": {
"type": "string",
"description": "Storage protocol or adapter (e.g., 'google-drive', 'ipfs', 'onedrive')."
},
"format": {
"type": "string",
"description": "Signature or proof container format (e.g., 'jws', 'cid', 'ledger-tx')."
},
"algorithms": {
"type": "array",
"items": { "type": "string" },
"description": "Supported signing or hashing algorithms for storage proofs."
},
"endpoint": {
"type": "string",
"format": "uri",
"description": "Optional API base URI or RPC endpoint for verification."
}
}
},
"coverScheme": {
"type": "object",
"properties": {
"supported": { "type": "boolean" },
"algorithms": {
"type": "array",
"items": { "type": "string" },
"description": "Supported algorithms for user cover signatures (e.g., ['ES256', 'secp256k1'])."
}
},
"required": ["supported"],
"description": "Optional description of supported user 'cover' signature schemes."
},
"algorithms": {
"type": "array",
"items": { "type": "string" },
"description": "Global list of all supported signature algorithms across this provider."
},
"contact": {
"type": "object",
"properties": {
"maintainer": { "type": "string" },
"email": { "type": "string", "format": "email" },
"url": { "type": "string", "format": "uri" }
},
"description": "Optional contact information for maintainers or sponsoring organization."
},
"deprecated": {
"type": "boolean",
"default": false,
"description": "Indicates whether this provider is deprecated but retained for historical verification."
},
"metadata": {
"type": "object",
"additionalProperties": true,
"description": "Optional arbitrary metadata for provider extensions."
},
"signature": {
"type": "object",
"description": "Optional detached digital signature proving descriptor authenticity.",
"properties": {
"format": { "type": "string", "enum": ["jws", "cose", "pgp"] },
"value": { "type": "string" },
"alg": { "type": "string" },
"issuer": { "type": "string" }
},
"required": ["format", "value"]
}
},
"additionalProperties": false
}
