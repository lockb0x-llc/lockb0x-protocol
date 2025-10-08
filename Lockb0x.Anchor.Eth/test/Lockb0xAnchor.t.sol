// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import "ds-test/test.sol";
import "../contracts/Lockb0xAnchor.sol";

contract Lockb0xAnchorTest is DSTest {
    Lockb0x_Anchor_Eth anchor;

    function setUp() public {
    anchor = new Lockb0x_Anchor_Eth();
    }

    function testAnchorAndRetrieve() public {
        bytes32 hash = keccak256(abi.encodePacked("test"));
        string memory metadata = "{\"name\":\"test\"}";
        anchor.anchor(hash, metadata);
        (bytes32 storedHash, string memory storedMetadata, address submitter, uint256 timestamp) = anchor.getAnchor(hash);
        assertEq(storedHash, hash);
        assertEq(storedMetadata, metadata);
        assertEq(submitter, address(this));
        assertGt(timestamp, 0);
    }

    function testDuplicateAnchorReverts() public {
        bytes32 hash = keccak256(abi.encodePacked("test"));
        anchor.anchor(hash, "meta");
        try anchor.anchor(hash, "meta2") {
            fail();
        } catch Error(string memory reason) {
            assertEq(reason, "Already anchored");
        }
    }

    function testGetAnchorNotFoundReverts() public {
        bytes32 hash = keccak256(abi.encodePacked("missing"));
        try anchor.getAnchor(hash) {
            fail();
        } catch Error(string memory reason) {
            assertEq(reason, "Not anchored");
        }
    }
}
