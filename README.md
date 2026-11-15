# Dynamic DNS client

[![Build Status](https://github.com/balazs-kis/dynamic-dns/actions/workflows/ci.yml/badge.svg?branch=main "Build Status")](https://github.com/balazs-kis/dynamic-dns/actions/workflows/ci.yml?query=branch%3Amain)
[![Coverage Status](https://codecov.io/gh/balazs-kis/dynamic-dns/branch/main/graph/badge.svg)](https://codecov.io/gh/balazs-kis/dynamic-dns)
[![License: MIT](https://img.shields.io/badge/license-MIT-blueviolet)](https://opensource.org/licenses/MIT)

A simple Dynamic DNS client for updating the public IP address of a server, written in C#.

## Usage
1. :floppy_disk: Download the [latest release](https://github.com/balazs-kis/dynamic-dns/releases/latest) for your platform (Windows x64, Linux x64, Linux ARM64 or macOS on Apple Silicon)
2. :pencil2: Add your dynamic DNS provider's info to the configuration file
3. :rocket: Run the executable (add `-s` or `--silent` for skipping trace-level messages, or `-v` or `--version` to print the version and exit)
4. :arrows_counterclockwise: _(optional)_ Set up a recurring job to run the executable periodically for you

## Configuration
The default `appsettings.json` configuration file beside the executable contains a template with examples. The instance config parameters can be repeated as necessary for multiple domins.

| Global config parameters | Description                                                                                |
| ------------------------ | ------------------------------------------------------------------------------------------ |
| `savedStateFilePath`     | the path where the last successfully updated IP address will be saved                      |
| `ipProviderUrls`         | the list of APIs that provide the caller's public IP address in a simple text response     |

| Instance config parameters | Description                                                                                            |
| -------------------------- | ------------------------------------------------------------------------------------------------------ |
| `DomainName`               | the domain name to update the IP for, e.g. `my-domain.com`                                             |
| `Hosts`                    | the list of host records to update the IP for (generally `@` for root and `*` for wildcard records)    |
| `DnsApiSecret`             | the API secret (if needed), issued by your dynamic DNS provider                                        |
| `DnsApiUrlTemplate`        | the API endpoint URL template for your dynamic DNS provider used to update the IP address for records  |
| `DnsApiSuccessMessage`     | a phrase that the content of the API response must include (optional: not checked if not set)          |

Placeholders in the `DnsApiUrlTemplate` config parameter:
- `{Host}`: the provided hosts will be substituted here when calling the provider's API
- `{Domain}`: the provided domain name will be substituted here when calling the provider's API
- `{Secret}`: the provided API secret will be substituted here when calling the provider's API
- `{NewIp}`: the public IP obtained from one of the provided `IpProviderUrls` will be substituted here when calling the provider's API

## Setting up a cron job for dynamic DNS IP update
The steps below work the same on Linux and macOS. Download the package for your platform (`linux_x64`, `linux_arm64` or `osx_arm64`) and extract it to your `~/dyn-dns` directory:
```console
user@server:~$ mkdir -p ~/dyn-dns && tar -xzf dynamic_dns_client_linux_x64.tar.gz -C ~/dyn-dns
```
Each release also contains a `SHA256SUMS.txt` file, so you can verify the download first with `sha256sum --check --ignore-missing SHA256SUMS.txt` (on macOS: compare the output of `shasum -a 256 dynamic_dns_client_osx_arm64.tar.gz` with the matching line in the file).

> :apple: **macOS:** the executable is not signed with an Apple Developer certificate. If you downloaded the package with a browser, macOS will refuse to run it ("Apple could not verify..."). Remove the download quarantine flag once after extracting:
> ```console
> user@mac:~$ xattr -d com.apple.quarantine ~/dyn-dns/DynamicDnsClient
> ```
> Packages downloaded with `curl` or `wget` are not quarantined, so this step is not needed for them.

After writing the necessary info to the `appsettings.json` file in the same directory, all you have to do to run the app periodically is to add the following line to your crontab:
```cron
*/5 * * * * cd ~/dyn-dns && ./DynamicDnsClient -s >> dyn-dns.log
```
This will run the application every 5 minutes, and write the output to a `dyn-dns.log` file.

## Updating to a new version
If you already have a previous version set up, you only need to replace the executable. Your configured `appsettings.json` and the saved state file (`savedStateFilePath`) stay as they are.

> :warning: Don't extract the whole package over your existing installation: the `appsettings.json` in the package is the default template, and it would overwrite your configuration.

**Linux and macOS:** download the new package for your platform, and extract only the executable into your installation directory (`~/dyn-dns` in the example above):
```console
user@server:~$ tar -xzf dynamic_dns_client_linux_x64.tar.gz -C ~/dyn-dns DynamicDnsClient
```
On macOS, if you downloaded the package with a browser, remove the quarantine flag from the new executable again, as described above.

**Windows:** download the new `dynamic_dns_client_win_x64.zip`, extract it to a temporary folder, and copy only `DynamicDnsClient.exe` over the old one in your installation folder.

If you run the application periodically (e.g. with the cron job above), there is nothing else to do: the next run will use the new version.
