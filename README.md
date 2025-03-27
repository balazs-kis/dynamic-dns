# Dynamic DNS client

[![Build Status](https://github.com/balazs-kis/dynamic-dns/workflows/ci-main/badge.svg "Build Status")](https://github.com/balazs-kis/dynamic-dns/actions?query=workflow%3A%22ci-main%22)
[![Coverage Status](https://codecov.io/gh/balazs-kis/dynamic-dns/branch/main/graph/badge.svg)](https://codecov.io/gh/balazs-kis/dynamic-dns)
[![License: MIT](https://img.shields.io/badge/license-MIT-blueviolet)](https://opensource.org/licenses/MIT)

A simple Dynamic DNS client for updating the public IP address of a server, written in C#.

## Usage
1. :floppy_disk: Download the latest release for your platform (Windows or Linux)
2. :pencil2: Add your dynamic DNS provider's info to the configuration file
3. :rocket: Run the executable (add `-s` or `--silent` for skipping trace-level messages)
4. :arrows_counterclockwise: _(optional)_ Set up a recurring job to run the executable periodically for you

## Configuration
The default `appsettings.json` configuration file beside the executable contains a template with examples. The instance config parameters can be repeated as necessary for multiple domins.

| Global Config parameters | Description                                                                                |
| ------------------------ | ------------------------------------------------------------------------------------------ |
| `savedStateFilePath`     | the path where the last successfully updated IP address will be saved                      |
| `ipProviderUrls`         | the list of APIs that provide the caller's public IP address in a simple text response     |

| Instance config parameters | Description                                                                                            |
| -------------------------- | ------------------------------------------------------------------------------------------------------ |
| `DomainName`               | the domain name to update the IP for, e.g. `my-domain.com`                                             |
| `Hosts`                    | the list of host records to update the IP for (generally `@` for root and `*` for wildcard records)    |
| `DnsApiSecret`             | the API secret (if needed), issued by your dynamic DNS provider                                        |
| `DnsApiUrlTemplate`        | the API endpoint URL template for your dynamic DNS provider used to update the IP address for records  |
| `DnsApiSuccessMessage`     | a phrase that the content of the API response must include (optional: not checked in not set)          |

Placeholders in the `DnsApiUrlTemplate` config parameter:
- `{Host}`: the provided hosts will be substituted here when calling the provider's API
- `{Domain}`: the provided domain name will be substituted here when calling the provider's API
- `{Secret}`: the provided API secret will be substituted here when calling the provider's API
- `{NewIp}`: the public IP obtained from one of the provided `IpProviderUrls` will be substituted here when calling the provider's API

## Setting up a cron job for dynamic DNS IP update
Let's assume that the Linux x64 version was downloaded and unzipped to your `~/dyn-dns` directory and the necessary info was written to the `.config` file in the same directory.
After all that, give execution rights to the application:
```console
user@server:~/dyn-dns$ sudo chmod +x DynamicDnsClient
```
All you have to do to run the app periodically is to add the following line to your crontab:
```cron
*/5 * * * * cd ~/dyn-dns && ./DynamicDnsClient -s >> dyn-dns.log
```
This will run the application every 5 minutes, and write the output to a `dyn-dns.log` file.
