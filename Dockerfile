FROM debian:bookworm-slim
WORKDIR /DeqqitApi

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libicu-dev \
    libgssapi-krb5-2 \
    libssl3 && \
    rm -rf /var/lib/apt/lists/*

# Copy the published binary from your local machine to the container
# COPY ./Api/bin/Release/net10.0/linux-x64/publish/Api .
COPY ./Api/bin/Release/net10.0/linux-x64/publish .
# COPY ./Api/bin/Release/net10.0/linux-x64/publish/TelegramBot .
COPY entrypoint .
# COPY ./.secrets .

RUN chmod +x Api entrypoint
# RUN chmod +x Api TelegramBot entrypoint

# Set Globalization Invariant mode to true
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Set the entry point to your application's DLL
ENTRYPOINT ["./entrypoint"]