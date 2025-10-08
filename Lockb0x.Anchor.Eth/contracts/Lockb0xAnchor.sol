// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

/// @title Lockb0x.Anchor.Eth - Ethereum Anchor for Content Integrity Proofs
/// @notice Stores hashes and metadata for off-chain artifacts
contract Lockb0x_Anchor_Eth {
    struct Anchor {
        bytes32 hash;
        string metadata;
        address submitter;
        uint256 timestamp;
    }

    mapping(bytes32 => Anchor) public anchors;
    event Anchored(bytes32 indexed hash, string metadata, address indexed submitter, uint256 timestamp);

    /// @notice Anchor a hash with metadata
    /// @param hash The hash of the artifact (sha256/keccak256)
    /// @param metadata Arbitrary metadata (e.g. JSON, URI)
    function anchor(bytes32 hash, string calldata metadata) external {
        require(anchors[hash].timestamp == 0, "Already anchored");
        anchors[hash] = Anchor(hash, metadata, msg.sender, block.timestamp);
        emit Anchored(hash, metadata, msg.sender, block.timestamp);
    }

    /// @notice Retrieve anchor details
    /// @param hash The hash to query
    function getAnchor(bytes32 hash) external view returns (Anchor memory) {
        require(anchors[hash].timestamp != 0, "Not anchored");
        return anchors[hash];
    }
}
